using BlogEngine.Core;
using BlogEngine.Core.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlogEngine.NET.AppCode
{
	/// <summary>
	/// Controller responsible for serving files (images, videos, etc.)
	/// </summary>
	public class FilesController : AsyncController
	{
		/// <summary>
		/// md5 hash
		/// </summary>
		static MD5 md5 = MD5.Create();

		/// <summary>
		/// 
		/// </summary>
		const string keyImagesProcessed = "cache-images-processed";

		/// <summary>
		/// hex digits for conversion
		/// </summary>
		private static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public string ToHexString(byte[] bytes)
		{
			var j = bytes.Length;
			var chars = new char[j * 2];

			for (int i = 0; i < j; i++)
			{
				var b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}

			return new string(chars);
		}

		/// <summary>
		/// List of Processed Images (from Cache)
		/// </summary>
		public ConcurrentDictionary<string, object> ImagesProcessed
		{
			get
			{
				var lt = this.HttpContext.Cache[keyImagesProcessed] as ConcurrentDictionary<string, object>;
				if (lt != null) return lt;

				try
				{
					return lt = new ConcurrentDictionary<string, object>();
				}
				finally
				{
					this.HttpContext.Cache[keyImagesProcessed] = lt;
				}
			}
		}

		/// <summary>
		/// file sizes
		/// </summary>
		protected enum ePictureSizes : short
		{
			/// <summary>
			/// thumbnail for mobile devices (25px)
			/// </summary>
			mobile_thumbnail = 25,

			/// <summary>
			/// small for mobile devices (100px)
			/// </summary>
			mobile_small = 100,

			/// <summary>
			/// medium for mobile devices (300px)
			/// </summary>
			mobile_medium = 300,

			/// <summary>
			/// medium for mobile devices (450px)
			/// </summary>
			mobile_large = 450,

			/// <summary>
			/// original (does not scale)
			/// </summary>
			original = 0,
			/// <summary>
			/// thumbnail (width: 50px)
			/// </summary>
			thumbnail = 50,
			/// <summary>
			/// small (width: 200px)
			/// </summary>
			small = 200,
			/// <summary>
			/// medium (width: 600px)
			/// </summary>
			medium = 600,
			/// <summary>
			/// medium (width: 900px)
			/// </summary>
			large = 900,
		}

		/// <summary>
		/// Directory where scaled images will be saved
		/// </summary>
		public string DirectoryForScalingImages
		{
			get { return $"{Blog.CurrentInstance.StorageLocation.TrimEnd('/')}/files/scaling/"; }
		}

		/// <summary>
		/// Returns the file
		/// </summary>
		/// <param name="filepath"></param>
		/// <param name="size"></param>
		[HttpGet, Route("~/files/{*filepath}")]
		public async Task<ActionResult> IndexAsync(string filepath, string size)
		{
			var realPath = $"{Blog.CurrentInstance.StorageLocation.TrimEnd('/')}/files/{filepath.TrimStart('/')}";
			if (Directory.Exists(realPath) == true)
			{
				var dirInfo = new DirectoryInfo(realPath);
				if (dirInfo.Root.Name.EndsWith(Blog.CurrentInstance.StorageLocation, StringComparison.CurrentCultureIgnoreCase) == false)
				{
					return await Task.FromResult(this.HttpNotFound($"Invalid Path: {filepath}"));
				}
			}

			var fileExists = BlogService.FileExists(realPath);
			if (fileExists)
			{
				var file = BlogService.GetFile(realPath);
				if (file.IsImage)
				{
					return await Task.FromResult(this.Picture(realPath, size));
				}
				else if (file.IsVideo)
				{
					return await Task.FromResult(this.Video(realPath, size));
				}
				else
				{
					return await Task.FromResult(this.Other(realPath, size));
				}
			}

			return await Task.FromResult(this.HttpNotFound(filepath));
		}

		/// <summary>
		/// Returns the image/picture
		/// </summary>
		/// <param name="realPath"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		FileContentResult Picture(string realPath, string size)
		{
			var file = BlogService.GetFile(realPath);
			var esize = ePictureSizes.original; Enum.TryParse<ePictureSizes>(size, true, out esize);

			try
			{
				//will use sizes for mobiles
				if (Utils.IsMobile)
				{
					switch (esize)
					{
						case ePictureSizes.large:
							esize = ePictureSizes.mobile_large;
							break;
						case ePictureSizes.medium:
							esize = ePictureSizes.mobile_medium;
							break;
						case ePictureSizes.small:
							esize = ePictureSizes.mobile_small;
							break;
						case ePictureSizes.thumbnail:
							esize = ePictureSizes.mobile_thumbnail;
							break;
					}
				}

				//image scaling
				if (file.IsImage && esize != ePictureSizes.original)
				{
					//first: get a md5 from file fullpath
					var bytes = (from c in file.FullPath select Convert.ToByte(c)).ToArray();
					var hash = this.ToHexString(md5.ComputeHash(bytes));

					//creates the scaling directory
					if (BlogService.DirectoryExists(this.DirectoryForScalingImages) == false) BlogService.CreateDirectory(this.DirectoryForScalingImages);

					var cmp = StringComparison.CurrentCultureIgnoreCase;

					/*scaled file*/
					var scaleimage = BlogService
						.GetDirectory(this.DirectoryForScalingImages)
						.Files
						.FirstOrDefault(f => f.Name.Equals($"{hash}.{esize}{file.Extension}", cmp));

					//scale image does not exist or is out dated
					if (scaleimage == null || scaleimage.DateCreated <= file.DateCreated || scaleimage.DateModified <= file.DateModified)
					{
						var fileexists = ScalePicture(file, hash, esize);

						scaleimage = fileexists ?
							BlogService.GetFile($"{this.DirectoryForScalingImages}{hash}.{esize.ToString().ToLower()}{file.Extension}") :
							null;
					}

					file = scaleimage ?? file;
				}

				var extension = file.Extension.TrimStart('.');
				return this.File(file.FileContents, string.Compare(extension, "JPG", true) == 0 ? "image/jpeg" : $"image/{extension}");
			}
			finally
			{
				using (var memstream = new MemoryStream(file.FileContents) { })
				{
					//image dimensions custom headers
					using (var bmp = new Bitmap(memstream) { })
					{
						var ih = bmp.Height.ToString();
						var iw = bmp.Width.ToString();

						this.HttpContext.Response.AddHeader("Image-Height", ih);
						this.HttpContext.Response.AddHeader("Image-Width", iw);
					}
				}
			}
		}

		/// <summary>
		/// It scales the image to return as thumbnails and so on (depending on the request)
		/// </summary>
		/// <returns></returns>
		protected bool ScalePicture(BlogEngine.Core.FileSystem.File file, string hash, ePictureSizes size)
		{
			if (file == null) return false;
			if (file.IsImage == false) return false;

			var filename = $"{hash}.{size}.{file.Extension.TrimStart('.')}".ToLower();

			if (ImagesProcessed.ContainsKey(filename) == false || ImagesProcessed[filename].Equals(false))
			{
				lock (ImagesProcessed)
				{
					if (ImagesProcessed.ContainsKey(filename) == false) ImagesProcessed[filename] = false;
				}

				lock (ImagesProcessed[filename])
				{
					if (ImagesProcessed[filename].Equals(false))
					{
						try
						{
							var realpath = this.HttpContext.Server.MapPath(file.FullPath);
							var noextension = file.Name.Replace(file.Extension, string.Empty);

							using (var image = Image.FromFile(realpath))
							{
								var w = (short)size;
								var f = 1 - ((float)(image.Size.Width - (short)size) / image.Size.Width);
								var h = (int)(image.Size.Height * f);

								var newImage = new Bitmap(w, h);
								using (newImage)
								{
									using (var g = Graphics.FromImage(newImage))
									{
										g.DrawImage(image, 0, 0, w, h);
									}

									//scaling directory
									var newpath = this.HttpContext.Server.MapPath(this.DirectoryForScalingImages);
									newImage.Save($"{newpath}\\{filename}");

									ImagesProcessed[filename] = true;
								}
							}

							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
				}
			}

			return BlogService.FileExists(this.DirectoryForScalingImages + filename);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="realPath"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		FileResult Video(string realPath, string size)
		{
			var file = BlogService.GetFile(realPath);
			return this.File(new MemoryStream(file.FileContents), $"video/{file.Extension.TrimStart('.')}");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="realPath"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		FileContentResult Other(string realPath, string size)
		{
			var file = BlogService.GetFile(realPath);
			var contentType = string.Empty;

			switch (file.Extension.ToLower())
			{
				case ".x3d": contentType = "application/vnd.hzn-3d-crossword"; break;
				case ".3gp": contentType = "video/3gpp"; break;
				case ".3g2": contentType = "video/3gpp2"; break;
				case ".mseq": contentType = "application/vnd.mseq"; break;
				case ".pwn": contentType = "application/vnd.3m.post-it-notes"; break;
				case ".plb": contentType = "application/vnd.3gpp.pic-bw-large"; break;
				case ".psb": contentType = "application/vnd.3gpp.pic-bw-small"; break;
				case ".pvb": contentType = "application/vnd.3gpp.pic-bw-var"; break;
				case ".tcap": contentType = "application/vnd.3gpp2.tcap"; break;
				case ".7z": contentType = "application/x-7z-compressed"; break;
				case ".abw": contentType = "application/x-abiword"; break;
				case ".ace": contentType = "application/x-ace-compressed"; break;
				case ".acc": contentType = "application/vnd.americandynamics.acc"; break;
				case ".acu": contentType = "application/vnd.acucobol"; break;
				case ".atc": contentType = "application/vnd.acucorp"; break;
				case ".adp": contentType = "audio/adpcm"; break;
				case ".aab": contentType = "application/x-authorware-bin"; break;
				case ".aam": contentType = "application/x-authorware-map"; break;
				case ".aas": contentType = "application/x-authorware-seg"; break;
				case ".air": contentType = "application/vnd.adobe.air-application-installer-package+zip"; break;
				case ".swf": contentType = "application/x-shockwave-flash"; break;
				case ".fxp": contentType = "application/vnd.adobe.fxp"; break;
				case ".pdf": contentType = "application/pdf"; break;
				case ".ppd": contentType = "application/vnd.cups-ppd"; break;
				case ".dir": contentType = "application/x-director"; break;
				case ".xdp": contentType = "application/vnd.adobe.xdp+xml"; break;
				case ".xfdf": contentType = "application/vnd.adobe.xfdf"; break;
				case ".aac": contentType = "audio/x-aac"; break;
				case ".ahead": contentType = "application/vnd.ahead.space"; break;
				case ".azf": contentType = "application/vnd.airzip.filesecure.azf"; break;
				case ".azs": contentType = "application/vnd.airzip.filesecure.azs"; break;
				case ".azw": contentType = "application/vnd.amazon.ebook"; break;
				case ".ami": contentType = "application/vnd.amiga.ami"; break;
				case ".apk": contentType = "application/vnd.android.package-archive"; break;
				case ".cii": contentType = "application/vnd.anser-web-certificate-issue-initiation"; break;
				case ".fti": contentType = "application/vnd.anser-web-funds-transfer-initiation"; break;
				case ".atx": contentType = "application/vnd.antix.game-component"; break;
				case ".dmg": contentType = "application/x-apple-diskimage"; break;
				case ".mpkg": contentType = "application/vnd.apple.installer+xml"; break;
				case ".aw": contentType = "application/applixware"; break;
				case ".les": contentType = "application/vnd.hhe.lesson-player"; break;
				case ".swi": contentType = "application/vnd.aristanetworks.swi"; break;
				case ".s": contentType = "text/x-asm"; break;
				case ".atomcat": contentType = "application/atomcat+xml"; break;
				case ".atomsvc": contentType = "application/atomsvc+xml"; break;
				case ".atom": contentType = "application/atom+xml"; break;
				case ".ac": contentType = "application/pkix-attr-cert"; break;
				case ".aif": contentType = "audio/x-aiff"; break;
				case ".avi": contentType = "video/x-msvideo"; break;
				case ".aep": contentType = "application/vnd.audiograph"; break;
				case ".dxf": contentType = "image/vnd.dxf"; break;
				case ".dwf": contentType = "model/vnd.dwf"; break;
				case ".par": contentType = "text/plain-bas"; break;
				case ".bcpio": contentType = "application/x-bcpio"; break;
				case ".bin": contentType = "application/octet-stream"; break;
				case ".bmp": contentType = "image/bmp"; break;
				case ".torrent": contentType = "application/x-bittorrent"; break;
				case ".cod": contentType = "application/vnd.rim.cod"; break;
				case ".mpm": contentType = "application/vnd.blueice.multipass"; break;
				case ".bmi": contentType = "application/vnd.bmi"; break;
				case ".sh": contentType = "application/x-sh"; break;
				case ".btif": contentType = "image/prs.btif"; break;
				case ".rep": contentType = "application/vnd.businessobjects"; break;
				case ".bz": contentType = "application/x-bzip"; break;
				case ".bz2": contentType = "application/x-bzip2"; break;
				case ".csh": contentType = "application/x-csh"; break;
				case ".c": contentType = "text/x-c"; break;
				case ".cdxml": contentType = "application/vnd.chemdraw+xml"; break;
				case ".css": contentType = "text/css"; break;
				case ".cdx": contentType = "chemical/x-cdx"; break;
				case ".cml": contentType = "chemical/x-cml"; break;
				case ".csml": contentType = "chemical/x-csml"; break;
				case ".cdbcmsg": contentType = "application/vnd.contact.cmsg"; break;
				case ".cla": contentType = "application/vnd.claymore"; break;
				case ".c4g": contentType = "application/vnd.clonk.c4group"; break;
				case ".sub": contentType = "image/vnd.dvb.subtitle"; break;
				case ".cdmia": contentType = "application/cdmi-capability"; break;
				case ".cdmic": contentType = "application/cdmi-container"; break;
				case ".cdmid": contentType = "application/cdmi-domain"; break;
				case ".cdmio": contentType = "application/cdmi-object"; break;
				case ".cdmiq": contentType = "application/cdmi-queue"; break;
				case ".c11amc": contentType = "application/vnd.cluetrust.cartomobile-config"; break;
				case ".c11amz": contentType = "application/vnd.cluetrust.cartomobile-config-pkg"; break;
				case ".ras": contentType = "image/x-cmu-raster"; break;
				case ".dae": contentType = "model/vnd.collada+xml"; break;
				case ".csv": contentType = "text/csv"; break;
				case ".cpt": contentType = "application/mac-compactpro"; break;
				case ".wmlc": contentType = "application/vnd.wap.wmlc"; break;
				case ".cgm": contentType = "image/cgm"; break;
				case ".ice": contentType = "x-conference/x-cooltalk"; break;
				case ".cmx": contentType = "image/x-cmx"; break;
				case ".xar": contentType = "application/vnd.xara"; break;
				case ".cmc": contentType = "application/vnd.cosmocaller"; break;
				case ".cpio": contentType = "application/x-cpio"; break;
				case ".clkx": contentType = "application/vnd.crick.clicker"; break;
				case ".clkk": contentType = "application/vnd.crick.clicker.keyboard"; break;
				case ".clkp": contentType = "application/vnd.crick.clicker.palette"; break;
				case ".clkt": contentType = "application/vnd.crick.clicker.template"; break;
				case ".clkw": contentType = "application/vnd.crick.clicker.wordbank"; break;
				case ".wbs": contentType = "application/vnd.criticaltools.wbs+xml"; break;
				case ".cryptonote": contentType = "application/vnd.rig.cryptonote"; break;
				case ".cif": contentType = "chemical/x-cif"; break;
				case ".cmdf": contentType = "chemical/x-cmdf"; break;
				case ".cu": contentType = "application/cu-seeme"; break;
				case ".cww": contentType = "application/prs.cww"; break;
				case ".curl": contentType = "text/vnd.curl"; break;
				case ".dcurl": contentType = "text/vnd.curl.dcurl"; break;
				case ".mcurl": contentType = "text/vnd.curl.mcurl"; break;
				case ".scurl": contentType = "text/vnd.curl.scurl"; break;
				case ".car": contentType = "application/vnd.curl.car"; break;
				case ".pcurl": contentType = "application/vnd.curl.pcurl"; break;
				case ".cmp": contentType = "application/vnd.yellowriver-custom-menu"; break;
				case ".dssc": contentType = "application/dssc+der"; break;
				case ".xdssc": contentType = "application/dssc+xml"; break;
				case ".deb": contentType = "application/x-debian-package"; break;
				case ".uva": contentType = "audio/vnd.dece.audio"; break;
				case ".uvi": contentType = "image/vnd.dece.graphic"; break;
				case ".uvh": contentType = "video/vnd.dece.hd"; break;
				case ".uvm": contentType = "video/vnd.dece.mobile"; break;
				case ".uvu": contentType = "video/vnd.uvvu.mp4"; break;
				case ".uvp": contentType = "video/vnd.dece.pd"; break;
				case ".uvs": contentType = "video/vnd.dece.sd"; break;
				case ".uvv": contentType = "video/vnd.dece.video"; break;
				case ".dvi": contentType = "application/x-dvi"; break;
				case ".seed": contentType = "application/vnd.fdsn.seed"; break;
				case ".dtb": contentType = "application/x-dtbook+xml"; break;
				case ".res": contentType = "application/x-dtbresource+xml"; break;
				case ".ait": contentType = "application/vnd.dvb.ait"; break;
				case ".svc": contentType = "application/vnd.dvb.service"; break;
				case ".eol": contentType = "audio/vnd.digital-winds"; break;
				case ".djvu": contentType = "image/vnd.djvu"; break;
				case ".dtd": contentType = "application/xml-dtd"; break;
				case ".mlp": contentType = "application/vnd.dolby.mlp"; break;
				case ".wad": contentType = "application/x-doom"; break;
				case ".dpg": contentType = "application/vnd.dpgraph"; break;
				case ".dra": contentType = "audio/vnd.dra"; break;
				case ".dfac": contentType = "application/vnd.dreamfactory"; break;
				case ".dts": contentType = "audio/vnd.dts"; break;
				case ".dtshd": contentType = "audio/vnd.dts.hd"; break;
				case ".dwg": contentType = "image/vnd.dwg"; break;
				case ".geo": contentType = "application/vnd.dynageo"; break;
				case ".es": contentType = "application/ecmascript"; break;
				case ".mag": contentType = "application/vnd.ecowin.chart"; break;
				case ".mmr": contentType = "image/vnd.fujixerox.edmics-mmr"; break;
				case ".rlc": contentType = "image/vnd.fujixerox.edmics-rlc"; break;
				case ".exi": contentType = "application/exi"; break;
				case ".mgz": contentType = "application/vnd.proteus.magazine"; break;
				case ".epub": contentType = "application/epub+zip"; break;
				case ".eml": contentType = "message/rfc822"; break;
				case ".nml": contentType = "application/vnd.enliven"; break;
				case ".xpr": contentType = "application/vnd.is-xpr"; break;
				case ".xif": contentType = "image/vnd.xiff"; break;
				case ".xfdl": contentType = "application/vnd.xfdl"; break;
				case ".emma": contentType = "application/emma+xml"; break;
				case ".ez2": contentType = "application/vnd.ezpix-album"; break;
				case ".ez3": contentType = "application/vnd.ezpix-package"; break;
				case ".fst": contentType = "image/vnd.fst"; break;
				case ".fvt": contentType = "video/vnd.fvt"; break;
				case ".fbs": contentType = "image/vnd.fastbidsheet"; break;
				case ".fe_launch": contentType = "application/vnd.denovo.fcselayout-link"; break;
				case ".f4v": contentType = "video/x-f4v"; break;
				case ".flv": contentType = "video/x-flv"; break;
				case ".fpx": contentType = "image/vnd.fpx"; break;
				case ".npx": contentType = "image/vnd.net-fpx"; break;
				case ".flx": contentType = "text/vnd.fmi.flexstor"; break;
				case ".fli": contentType = "video/x-fli"; break;
				case ".ftc": contentType = "application/vnd.fluxtime.clip"; break;
				case ".fdf": contentType = "application/vnd.fdf"; break;
				case ".f": contentType = "text/x-fortran"; break;
				case ".mif": contentType = "application/vnd.mif"; break;
				case ".fm": contentType = "application/vnd.framemaker"; break;
				case ".fh": contentType = "image/x-freehand"; break;
				case ".fsc": contentType = "application/vnd.fsc.weblaunch"; break;
				case ".fnc": contentType = "application/vnd.frogans.fnc"; break;
				case ".ltf": contentType = "application/vnd.frogans.ltf"; break;
				case ".ddd": contentType = "application/vnd.fujixerox.ddd"; break;
				case ".xdw": contentType = "application/vnd.fujixerox.docuworks"; break;
				case ".xbd": contentType = "application/vnd.fujixerox.docuworks.binder"; break;
				case ".oas": contentType = "application/vnd.fujitsu.oasys"; break;
				case ".oa2": contentType = "application/vnd.fujitsu.oasys2"; break;
				case ".oa3": contentType = "application/vnd.fujitsu.oasys3"; break;
				case ".fg5": contentType = "application/vnd.fujitsu.oasysgp"; break;
				case ".bh2": contentType = "application/vnd.fujitsu.oasysprs"; break;
				case ".spl": contentType = "application/x-futuresplash"; break;
				case ".fzs": contentType = "application/vnd.fuzzysheet"; break;
				case ".g3": contentType = "image/g3fax"; break;
				case ".gmx": contentType = "application/vnd.gmx"; break;
				case ".gtw": contentType = "model/vnd.gtw"; break;
				case ".txd": contentType = "application/vnd.genomatix.tuxedo"; break;
				case ".ggb": contentType = "application/vnd.geogebra.file"; break;
				case ".ggt": contentType = "application/vnd.geogebra.tool"; break;
				case ".gdl": contentType = "model/vnd.gdl"; break;
				case ".gex": contentType = "application/vnd.geometry-explorer"; break;
				case ".gxt": contentType = "application/vnd.geonext"; break;
				case ".g2w": contentType = "application/vnd.geoplan"; break;
				case ".g3w": contentType = "application/vnd.geospace"; break;
				case ".gsf": contentType = "application/x-font-ghostscript"; break;
				case ".bdf": contentType = "application/x-font-bdf"; break;
				case ".gtar": contentType = "application/x-gtar"; break;
				case ".texinfo": contentType = "application/x-texinfo"; break;
				case ".gnumeric": contentType = "application/x-gnumeric"; break;
				case ".kml": contentType = "application/vnd.google-earth.kml+xml"; break;
				case ".kmz": contentType = "application/vnd.google-earth.kmz"; break;
				case ".gqf": contentType = "application/vnd.grafeq"; break;
				case ".gif": contentType = "image/gif"; break;
				case ".gv": contentType = "text/vnd.graphviz"; break;
				case ".gac": contentType = "application/vnd.groove-account"; break;
				case ".ghf": contentType = "application/vnd.groove-help"; break;
				case ".gim": contentType = "application/vnd.groove-identity-message"; break;
				case ".grv": contentType = "application/vnd.groove-injector"; break;
				case ".gtm": contentType = "application/vnd.groove-tool-message"; break;
				case ".tpl": contentType = "application/vnd.groove-tool-template"; break;
				case ".vcg": contentType = "application/vnd.groove-vcard"; break;
				case ".h261": contentType = "video/h261"; break;
				case ".h263": contentType = "video/h263"; break;
				case ".h264": contentType = "video/h264"; break;
				case ".hpid": contentType = "application/vnd.hp-hpid"; break;
				case ".hps": contentType = "application/vnd.hp-hps"; break;
				case ".hdf": contentType = "application/x-hdf"; break;
				case ".rip": contentType = "audio/vnd.rip"; break;
				case ".hbci": contentType = "application/vnd.hbci"; break;
				case ".jlt": contentType = "application/vnd.hp-jlyt"; break;
				case ".pcl": contentType = "application/vnd.hp-pcl"; break;
				case ".hpgl": contentType = "application/vnd.hp-hpgl"; break;
				case ".hvs": contentType = "application/vnd.yamaha.hv-script"; break;
				case ".hvd": contentType = "application/vnd.yamaha.hv-dic"; break;
				case ".hvp": contentType = "application/vnd.yamaha.hv-voice"; break;
				case ".sfd-hdstx": contentType = "application/vnd.hydrostatix.sof-data"; break;
				case ".stk": contentType = "application/hyperstudio"; break;
				case ".hal": contentType = "application/vnd.hal+xml"; break;
				case ".html": contentType = "text/html"; break;
				case ".irm": contentType = "application/vnd.ibm.rights-management"; break;
				case ".sc": contentType = "application/vnd.ibm.secure-container"; break;
				case ".ics": contentType = "text/calendar"; break;
				case ".icc": contentType = "application/vnd.iccprofile"; break;
				case ".ico": contentType = "image/x-icon"; break;
				case ".igl": contentType = "application/vnd.igloader"; break;
				case ".ief": contentType = "image/ief"; break;
				case ".ivp": contentType = "application/vnd.immervision-ivp"; break;
				case ".ivu": contentType = "application/vnd.immervision-ivu"; break;
				case ".rif": contentType = "application/reginfo+xml"; break;
				case ".3dml": contentType = "text/vnd.in3d.3dml"; break;
				case ".spot": contentType = "text/vnd.in3d.spot"; break;
				case ".igs": contentType = "model/iges"; break;
				case ".i2g": contentType = "application/vnd.intergeo"; break;
				case ".cdy": contentType = "application/vnd.cinderella"; break;
				case ".xpw": contentType = "application/vnd.intercon.formnet"; break;
				case ".fcs": contentType = "application/vnd.isac.fcs"; break;
				case ".ipfix": contentType = "application/ipfix"; break;
				case ".cer": contentType = "application/pkix-cert"; break;
				case ".pki": contentType = "application/pkixcmp"; break;
				case ".crl": contentType = "application/pkix-crl"; break;
				case ".pkipath": contentType = "application/pkix-pkipath"; break;
				case ".igm": contentType = "application/vnd.insors.igm"; break;
				case ".rcprofile": contentType = "application/vnd.ipunplugged.rcprofile"; break;
				case ".irp": contentType = "application/vnd.irepository.package+xml"; break;
				case ".jad": contentType = "text/vnd.sun.j2me.app-descriptor"; break;
				case ".jar": contentType = "application/java-archive"; break;
				case ".class": contentType = "application/java-vm"; break;
				case ".jnlp": contentType = "application/x-java-jnlp-file"; break;
				case ".ser": contentType = "application/java-serialized-object"; break;
				case ".java": contentType = "text/x-java-source,java"; break;
				case ".js": contentType = "application/javascript"; break;
				case ".json": contentType = "application/json"; break;
				case ".joda": contentType = "application/vnd.joost.joda-archive"; break;
				case ".jpm": contentType = "video/jpm"; break;
				case ".jpeg": case ".jpg": contentType = "image/jpeg"; break;
				case ".pjpeg": contentType = "image/pjpeg"; break;
				case ".jpgv": contentType = "video/jpeg"; break;
				case ".ktz": contentType = "application/vnd.kahootz"; break;
				case ".mmd": contentType = "application/vnd.chipnuts.karaoke-mmd"; break;
				case ".karbon": contentType = "application/vnd.kde.karbon"; break;
				case ".chrt": contentType = "application/vnd.kde.kchart"; break;
				case ".kfo": contentType = "application/vnd.kde.kformula"; break;
				case ".flw": contentType = "application/vnd.kde.kivio"; break;
				case ".kon": contentType = "application/vnd.kde.kontour"; break;
				case ".kpr": contentType = "application/vnd.kde.kpresenter"; break;
				case ".ksp": contentType = "application/vnd.kde.kspread"; break;
				case ".kwd": contentType = "application/vnd.kde.kword"; break;
				case ".htke": contentType = "application/vnd.kenameaapp"; break;
				case ".kia": contentType = "application/vnd.kidspiration"; break;
				case ".kne": contentType = "application/vnd.kinar"; break;
				case ".sse": contentType = "application/vnd.kodak-descriptor"; break;
				case ".lasxml": contentType = "application/vnd.las.las+xml"; break;
				case ".latex": contentType = "application/x-latex"; break;
				case ".lbd": contentType = "application/vnd.llamagraphics.life-balance.desktop"; break;
				case ".lbe": contentType = "application/vnd.llamagraphics.life-balance.exchange+xml"; break;
				case ".jam": contentType = "application/vnd.jam"; break;
				case "0.123": contentType = "application/vnd.lotus-1-2-3"; break;
				case ".apr": contentType = "application/vnd.lotus-approach"; break;
				case ".pre": contentType = "application/vnd.lotus-freelance"; break;
				case ".nsf": contentType = "application/vnd.lotus-notes"; break;
				case ".org": contentType = "application/vnd.lotus-organizer"; break;
				case ".scm": contentType = "application/vnd.lotus-screencam"; break;
				case ".lwp": contentType = "application/vnd.lotus-wordpro"; break;
				case ".lvp": contentType = "audio/vnd.lucent.voice"; break;
				case ".m3u": contentType = "audio/x-mpegurl"; break;
				case ".m4v": contentType = "video/x-m4v"; break;
				case ".hqx": contentType = "application/mac-binhex40"; break;
				case ".portpkg": contentType = "application/vnd.macports.portpkg"; break;
				case ".mgp": contentType = "application/vnd.osgeo.mapguide.package"; break;
				case ".mrc": contentType = "application/marc"; break;
				case ".mrcx": contentType = "application/marcxml+xml"; break;
				case ".mxf": contentType = "application/mxf"; break;
				case ".nbp": contentType = "application/vnd.wolfram.player"; break;
				case ".ma": contentType = "application/mathematica"; break;
				case ".mathml": contentType = "application/mathml+xml"; break;
				case ".mbox": contentType = "application/mbox"; break;
				case ".mc1": contentType = "application/vnd.medcalcdata"; break;
				case ".mscml": contentType = "application/mediaservercontrol+xml"; break;
				case ".cdkey": contentType = "application/vnd.mediastation.cdkey"; break;
				case ".mwf": contentType = "application/vnd.mfer"; break;
				case ".mfm": contentType = "application/vnd.mfmp"; break;
				case ".msh": contentType = "model/mesh"; break;
				case ".mads": contentType = "application/mads+xml"; break;
				case ".mets": contentType = "application/mets+xml"; break;
				case ".mods": contentType = "application/mods+xml"; break;
				case ".meta4": contentType = "application/metalink4+xml"; break;
				case ".mcd": contentType = "application/vnd.mcd"; break;
				case ".flo": contentType = "application/vnd.micrografx.flo"; break;
				case ".igx": contentType = "application/vnd.micrografx.igx"; break;
				case ".es3": contentType = "application/vnd.eszigno3+xml"; break;
				case ".mdb": contentType = "application/x-msaccess"; break;
				case ".asf": contentType = "video/x-ms-asf"; break;
				case ".exe": contentType = "application/x-msdownload"; break;
				case ".cil": contentType = "application/vnd.ms-artgalry"; break;
				case ".cab": contentType = "application/vnd.ms-cab-compressed"; break;
				case ".ims": contentType = "application/vnd.ms-ims"; break;
				case ".application": contentType = "application/x-ms-application"; break;
				case ".clp": contentType = "application/x-msclip"; break;
				case ".mdi": contentType = "image/vnd.ms-modi"; break;
				case ".eot": contentType = "application/vnd.ms-fontobject"; break;
				case ".xls": contentType = "application/vnd.ms-excel"; break;
				case ".xlam": contentType = "application/vnd.ms-excel.addin.macroenabled.12"; break;
				case ".xlsb": contentType = "application/vnd.ms-excel.sheet.binary.macroenabled.12"; break;
				case ".xltm": contentType = "application/vnd.ms-excel.template.macroenabled.12"; break;
				case ".xlsm": contentType = "application/vnd.ms-excel.sheet.macroenabled.12"; break;
				case ".chm": contentType = "application/vnd.ms-htmlhelp"; break;
				case ".crd": contentType = "application/x-mscardfile"; break;
				case ".lrm": contentType = "application/vnd.ms-lrm"; break;
				case ".mvb": contentType = "application/x-msmediaview"; break;
				case ".mny": contentType = "application/x-msmoney"; break;
				case ".pptx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation"; break;
				case ".sldx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.slide"; break;
				case ".ppsx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.slideshow"; break;
				case ".potx": contentType = "application/vnd.openxmlformats-officedocument.presentationml.template"; break;
				case ".xlsx": contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
				case ".xltx": contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.template"; break;
				case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
				case ".dotx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template"; break;
				case ".obd": contentType = "application/x-msbinder"; break;
				case ".thmx": contentType = "application/vnd.ms-officetheme"; break;
				case ".onetoc": contentType = "application/onenote"; break;
				case ".pya": contentType = "audio/vnd.ms-playready.media.pya"; break;
				case ".pyv": contentType = "video/vnd.ms-playready.media.pyv"; break;
				case ".ppt": contentType = "application/vnd.ms-powerpoint"; break;
				case ".ppam": contentType = "application/vnd.ms-powerpoint.addin.macroenabled.12"; break;
				case ".sldm": contentType = "application/vnd.ms-powerpoint.slide.macroenabled.12"; break;
				case ".pptm": contentType = "application/vnd.ms-powerpoint.presentation.macroenabled.12"; break;
				case ".ppsm": contentType = "application/vnd.ms-powerpoint.slideshow.macroenabled.12"; break;
				case ".potm": contentType = "application/vnd.ms-powerpoint.template.macroenabled.12"; break;
				case ".mpp": contentType = "application/vnd.ms-project"; break;
				case ".pub": contentType = "application/x-mspublisher"; break;
				case ".scd": contentType = "application/x-msschedule"; break;
				case ".xap": contentType = "application/x-silverlight-app"; break;
				case ".stl": contentType = "application/vnd.ms-pki.stl"; break;
				case ".cat": contentType = "application/vnd.ms-pki.seccat"; break;
				case ".vsd": contentType = "application/vnd.visio"; break;
				case ".vsdx": contentType = "application/vnd.visio2013"; break;
				case ".wm": contentType = "video/x-ms-wm"; break;
				case ".wma": contentType = "audio/x-ms-wma"; break;
				case ".wax": contentType = "audio/x-ms-wax"; break;
				case ".wmx": contentType = "video/x-ms-wmx"; break;
				case ".wmd": contentType = "application/x-ms-wmd"; break;
				case ".wpl": contentType = "application/vnd.ms-wpl"; break;
				case ".wmz": contentType = "application/x-ms-wmz"; break;
				case ".wmv": contentType = "video/x-ms-wmv"; break;
				case ".wvx": contentType = "video/x-ms-wvx"; break;
				case ".wmf": contentType = "application/x-msmetafile"; break;
				case ".trm": contentType = "application/x-msterminal"; break;
				case ".doc": contentType = "application/msword"; break;
				case ".docm": contentType = "application/vnd.ms-word.document.macroenabled.12"; break;
				case ".dotm": contentType = "application/vnd.ms-word.template.macroenabled.12"; break;
				case ".wri": contentType = "application/x-mswrite"; break;
				case ".wps": contentType = "application/vnd.ms-works"; break;
				case ".xbap": contentType = "application/x-ms-xbap"; break;
				case ".xps": contentType = "application/vnd.ms-xpsdocument"; break;
				case ".mid": contentType = "audio/midi"; break;
				case ".mpy": contentType = "application/vnd.ibm.minipay"; break;
				case ".afp": contentType = "application/vnd.ibm.modcap"; break;
				case ".rms": contentType = "application/vnd.jcp.javame.midlet-rms"; break;
				case ".tmo": contentType = "application/vnd.tmobile-livetv"; break;
				case ".prc": contentType = "application/x-mobipocket-ebook"; break;
				case ".mbk": contentType = "application/vnd.mobius.mbk"; break;
				case ".dis": contentType = "application/vnd.mobius.dis"; break;
				case ".plc": contentType = "application/vnd.mobius.plc"; break;
				case ".mqy": contentType = "application/vnd.mobius.mqy"; break;
				case ".msl": contentType = "application/vnd.mobius.msl"; break;
				case ".txf": contentType = "application/vnd.mobius.txf"; break;
				case ".daf": contentType = "application/vnd.mobius.daf"; break;
				case ".fly": contentType = "text/vnd.fly"; break;
				case ".mpc": contentType = "application/vnd.mophun.certificate"; break;
				case ".mpn": contentType = "application/vnd.mophun.application"; break;
				case ".mj2": contentType = "video/mj2"; break;
				case ".mpga": contentType = "audio/mpeg"; break;
				case ".mxu": contentType = "video/vnd.mpegurl"; break;
				case ".mpeg": contentType = "video/mpeg"; break;
				case ".m21": contentType = "application/mp21"; break;
				case ".mp4a": contentType = "audio/mp4"; break;
				case ".mp4": contentType = "video/mp4"; break;
				case ".m3u8": contentType = "application/vnd.apple.mpegurl"; break;
				case ".mus": contentType = "application/vnd.musician"; break;
				case ".msty": contentType = "application/vnd.muvee.style"; break;
				case ".mxml": contentType = "application/xv+xml"; break;
				case ".ngdat": contentType = "application/vnd.nokia.n-gage.data"; break;
				case ".n-gage": contentType = "application/vnd.nokia.n-gage.symbian.install"; break;
				case ".ncx": contentType = "application/x-dtbncx+xml"; break;
				case ".nc": contentType = "application/x-netcdf"; break;
				case ".nlu": contentType = "application/vnd.neurolanguage.nlu"; break;
				case ".dna": contentType = "application/vnd.dna"; break;
				case ".nnd": contentType = "application/vnd.noblenet-directory"; break;
				case ".nns": contentType = "application/vnd.noblenet-sealer"; break;
				case ".nnw": contentType = "application/vnd.noblenet-web"; break;
				case ".rpst": contentType = "application/vnd.nokia.radio-preset"; break;
				case ".rpss": contentType = "application/vnd.nokia.radio-presets"; break;
				case ".n3": contentType = "text/n3"; break;
				case ".edm": contentType = "application/vnd.novadigm.edm"; break;
				case ".edx": contentType = "application/vnd.novadigm.edx"; break;
				case ".ext": contentType = "application/vnd.novadigm.ext"; break;
				case ".gph": contentType = "application/vnd.flographit"; break;
				case ".ecelp4800": contentType = "audio/vnd.nuera.ecelp4800"; break;
				case ".ecelp7470": contentType = "audio/vnd.nuera.ecelp7470"; break;
				case ".ecelp9600": contentType = "audio/vnd.nuera.ecelp9600"; break;
				case ".oda": contentType = "application/oda"; break;
				case ".ogx": contentType = "application/ogg"; break;
				case ".oga": contentType = "audio/ogg"; break;
				case ".ogv": contentType = "video/ogg"; break;
				case ".dd2": contentType = "application/vnd.oma.dd2+xml"; break;
				case ".oth": contentType = "application/vnd.oasis.opendocument.text-web"; break;
				case ".opf": contentType = "application/oebps-package+xml"; break;
				case ".qbo": contentType = "application/vnd.intu.qbo"; break;
				case ".oxt": contentType = "application/vnd.openofficeorg.extension"; break;
				case ".osf": contentType = "application/vnd.yamaha.openscoreformat"; break;
				case ".weba": contentType = "audio/webm"; break;
				case ".webm": contentType = "video/webm"; break;
				case ".odc": contentType = "application/vnd.oasis.opendocument.chart"; break;
				case ".otc": contentType = "application/vnd.oasis.opendocument.chart-template"; break;
				case ".odb": contentType = "application/vnd.oasis.opendocument.database"; break;
				case ".odf": contentType = "application/vnd.oasis.opendocument.formula"; break;
				case ".odft": contentType = "application/vnd.oasis.opendocument.formula-template"; break;
				case ".odg": contentType = "application/vnd.oasis.opendocument.graphics"; break;
				case ".otg": contentType = "application/vnd.oasis.opendocument.graphics-template"; break;
				case ".odi": contentType = "application/vnd.oasis.opendocument.image"; break;
				case ".oti": contentType = "application/vnd.oasis.opendocument.image-template"; break;
				case ".odp": contentType = "application/vnd.oasis.opendocument.presentation"; break;
				case ".otp": contentType = "application/vnd.oasis.opendocument.presentation-template"; break;
				case ".ods": contentType = "application/vnd.oasis.opendocument.spreadsheet"; break;
				case ".ots": contentType = "application/vnd.oasis.opendocument.spreadsheet-template"; break;
				case ".odt": contentType = "application/vnd.oasis.opendocument.text"; break;
				case ".odm": contentType = "application/vnd.oasis.opendocument.text-master"; break;
				case ".ott": contentType = "application/vnd.oasis.opendocument.text-template"; break;
				case ".ktx": contentType = "image/ktx"; break;
				case ".sxc": contentType = "application/vnd.sun.xml.calc"; break;
				case ".stc": contentType = "application/vnd.sun.xml.calc.template"; break;
				case ".sxd": contentType = "application/vnd.sun.xml.draw"; break;
				case ".std": contentType = "application/vnd.sun.xml.draw.template"; break;
				case ".sxi": contentType = "application/vnd.sun.xml.impress"; break;
				case ".sti": contentType = "application/vnd.sun.xml.impress.template"; break;
				case ".sxm": contentType = "application/vnd.sun.xml.math"; break;
				case ".sxw": contentType = "application/vnd.sun.xml.writer"; break;
				case ".sxg": contentType = "application/vnd.sun.xml.writer.global"; break;
				case ".stw": contentType = "application/vnd.sun.xml.writer.template"; break;
				case ".otf": contentType = "application/x-font-otf"; break;
				case ".osfpvg": contentType = "application/vnd.yamaha.openscoreformat.osfpvg+xml"; break;
				case ".dp": contentType = "application/vnd.osgi.dp"; break;
				case ".pdb": contentType = "application/vnd.palm"; break;
				case ".p": contentType = "text/x-pascal"; break;
				case ".paw": contentType = "application/vnd.pawaafile"; break;
				case ".pclxl": contentType = "application/vnd.hp-pclxl"; break;
				case ".efif": contentType = "application/vnd.picsel"; break;
				case ".pcx": contentType = "image/x-pcx"; break;
				case ".psd": contentType = "image/vnd.adobe.photoshop"; break;
				case ".prf": contentType = "application/pics-rules"; break;
				case ".pic": contentType = "image/x-pict"; break;
				case ".chat": contentType = "application/x-chat"; break;
				case ".p10": contentType = "application/pkcs10"; break;
				case ".p12": contentType = "application/x-pkcs12"; break;
				case ".p7m": contentType = "application/pkcs7-mime"; break;
				case ".p7s": contentType = "application/pkcs7-signature"; break;
				case ".p7r": contentType = "application/x-pkcs7-certreqresp"; break;
				case ".p7b": contentType = "application/x-pkcs7-certificates"; break;
				case ".p8": contentType = "application/pkcs8"; break;
				case ".plf": contentType = "application/vnd.pocketlearn"; break;
				case ".pnm": contentType = "image/x-portable-anymap"; break;
				case ".pbm": contentType = "image/x-portable-bitmap"; break;
				case ".pcf": contentType = "application/x-font-pcf"; break;
				case ".pfr": contentType = "application/font-tdpfr"; break;
				case ".pgn": contentType = "application/x-chess-pgn"; break;
				case ".pgm": contentType = "image/x-portable-graymap"; break;
				case ".png": contentType = "image/png"; break;
				case ".ppm": contentType = "image/x-portable-pixmap"; break;
				case ".pskcxml": contentType = "application/pskc+xml"; break;
				case ".pml": contentType = "application/vnd.ctc-posml"; break;
				case ".ai": contentType = "application/postscript"; break;
				case ".pfa": contentType = "application/x-font-type1"; break;
				case ".pbd": contentType = "application/vnd.powerbuilder6"; break;
				case ".pgp": contentType = "application/pgp-encrypted"; break;
				case ".box": contentType = "application/vnd.previewsystems.box"; break;
				case ".ptid": contentType = "application/vnd.pvi.ptid1"; break;
				case ".pls": contentType = "application/pls+xml"; break;
				case ".str": contentType = "application/vnd.pg.format"; break;
				case ".ei6": contentType = "application/vnd.pg.osasli"; break;
				case ".dsc": contentType = "text/prs.lines.tag"; break;
				case ".psf": contentType = "application/x-font-linux-psf"; break;
				case ".qps": contentType = "application/vnd.publishare-delta-tree"; break;
				case ".wg": contentType = "application/vnd.pmi.widget"; break;
				case ".qxd": contentType = "application/vnd.quark.quarkxpress"; break;
				case ".esf": contentType = "application/vnd.epson.esf"; break;
				case ".msf": contentType = "application/vnd.epson.msf"; break;
				case ".ssf": contentType = "application/vnd.epson.ssf"; break;
				case ".qam": contentType = "application/vnd.epson.quickanime"; break;
				case ".qfx": contentType = "application/vnd.intu.qfx"; break;
				case ".qt": contentType = "video/quicktime"; break;
				case ".rar": contentType = "application/x-rar-compressed"; break;
				case ".ram": contentType = "audio/x-pn-realaudio"; break;
				case ".rmp": contentType = "audio/x-pn-realaudio-plugin"; break;
				case ".rsd": contentType = "application/rsd+xml"; break;
				case ".rm": contentType = "application/vnd.rn-realmedia"; break;
				case ".bed": contentType = "application/vnd.realvnc.bed"; break;
				case ".mxl": contentType = "application/vnd.recordare.musicxml"; break;
				case ".musicxml": contentType = "application/vnd.recordare.musicxml+xml"; break;
				case ".rnc": contentType = "application/relax-ng-compact-syntax"; break;
				case ".rdz": contentType = "application/vnd.data-vision.rdz"; break;
				case ".rdf": contentType = "application/rdf+xml"; break;
				case ".rp9": contentType = "application/vnd.cloanto.rp9"; break;
				case ".jisp": contentType = "application/vnd.jisp"; break;
				case ".rtf": contentType = "application/rtf"; break;
				case ".rtx": contentType = "text/richtext"; break;
				case ".link66": contentType = "application/vnd.route66.link66+xml"; break;
				case ".rss": contentType = "application/rss+xml"; break;
				case ".shf": contentType = "application/shf+xml"; break;
				case ".st": contentType = "application/vnd.sailingtracker.track"; break;
				case ".svg": contentType = "image/svg+xml"; break;
				case ".sus": contentType = "application/vnd.sus-calendar"; break;
				case ".sru": contentType = "application/sru+xml"; break;
				case ".setpay": contentType = "application/set-payment-initiation"; break;
				case ".setreg": contentType = "application/set-registration-initiation"; break;
				case ".sema": contentType = "application/vnd.sema"; break;
				case ".semd": contentType = "application/vnd.semd"; break;
				case ".semf": contentType = "application/vnd.semf"; break;
				case ".see": contentType = "application/vnd.seemail"; break;
				case ".snf": contentType = "application/x-font-snf"; break;
				case ".spq": contentType = "application/scvp-vp-request"; break;
				case ".spp": contentType = "application/scvp-vp-response"; break;
				case ".scq": contentType = "application/scvp-cv-request"; break;
				case ".scs": contentType = "application/scvp-cv-response"; break;
				case ".sdp": contentType = "application/sdp"; break;
				case ".etx": contentType = "text/x-setext"; break;
				case ".movie": contentType = "video/x-sgi-movie"; break;
				case ".ifm": contentType = "application/vnd.shana.informed.formdata"; break;
				case ".itp": contentType = "application/vnd.shana.informed.formtemplate"; break;
				case ".iif": contentType = "application/vnd.shana.informed.interchange"; break;
				case ".ipk": contentType = "application/vnd.shana.informed.package"; break;
				case ".tfi": contentType = "application/thraud+xml"; break;
				case ".shar": contentType = "application/x-shar"; break;
				case ".rgb": contentType = "image/x-rgb"; break;
				case ".slt": contentType = "application/vnd.epson.salt"; break;
				case ".aso": contentType = "application/vnd.accpac.simply.aso"; break;
				case ".imp": contentType = "application/vnd.accpac.simply.imp"; break;
				case ".twd": contentType = "application/vnd.simtech-mindmapper"; break;
				case ".csp": contentType = "application/vnd.commonspace"; break;
				case ".saf": contentType = "application/vnd.yamaha.smaf-audio"; break;
				case ".mmf": contentType = "application/vnd.smaf"; break;
				case ".spf": contentType = "application/vnd.yamaha.smaf-phrase"; break;
				case ".teacher": contentType = "application/vnd.smart.teacher"; break;
				case ".svd": contentType = "application/vnd.svd"; break;
				case ".rq": contentType = "application/sparql-query"; break;
				case ".srx": contentType = "application/sparql-results+xml"; break;
				case ".gram": contentType = "application/srgs"; break;
				case ".grxml": contentType = "application/srgs+xml"; break;
				case ".ssml": contentType = "application/ssml+xml"; break;
				case ".skp": contentType = "application/vnd.koan"; break;
				case ".sgml": contentType = "text/sgml"; break;
				case ".sdc": contentType = "application/vnd.stardivision.calc"; break;
				case ".sda": contentType = "application/vnd.stardivision.draw"; break;
				case ".sdd": contentType = "application/vnd.stardivision.impress"; break;
				case ".smf": contentType = "application/vnd.stardivision.math"; break;
				case ".sdw": contentType = "application/vnd.stardivision.writer"; break;
				case ".sgl": contentType = "application/vnd.stardivision.writer-global"; break;
				case ".sm": contentType = "application/vnd.stepmania.stepchart"; break;
				case ".sit": contentType = "application/x-stuffit"; break;
				case ".sitx": contentType = "application/x-stuffitx"; break;
				case ".sdkm": contentType = "application/vnd.solent.sdkm+xml"; break;
				case ".xo": contentType = "application/vnd.olpc-sugar"; break;
				case ".au": contentType = "audio/basic"; break;
				case ".wqd": contentType = "application/vnd.wqd"; break;
				case ".sis": contentType = "application/vnd.symbian.install"; break;
				case ".smi": contentType = "application/smil+xml"; break;
				case ".xsm": contentType = "application/vnd.syncml+xml"; break;
				case ".bdm": contentType = "application/vnd.syncml.dm+wbxml"; break;
				case ".xdm": contentType = "application/vnd.syncml.dm+xml"; break;
				case ".sv4cpio": contentType = "application/x-sv4cpio"; break;
				case ".sv4crc": contentType = "application/x-sv4crc"; break;
				case ".sbml": contentType = "application/sbml+xml"; break;
				case ".tsv": contentType = "text/tab-separated-values"; break;
				case ".tiff": contentType = "image/tiff"; break;
				case ".tao": contentType = "application/vnd.tao.intent-module-archive"; break;
				case ".tar": contentType = "application/x-tar"; break;
				case ".tcl": contentType = "application/x-tcl"; break;
				case ".tex": contentType = "application/x-tex"; break;
				case ".tfm": contentType = "application/x-tex-tfm"; break;
				case ".tei": contentType = "application/tei+xml"; break;
				case ".txt": contentType = "text/plain"; break;
				case ".dxp": contentType = "application/vnd.spotfire.dxp"; break;
				case ".sfs": contentType = "application/vnd.spotfire.sfs"; break;
				case ".tsd": contentType = "application/timestamped-data"; break;
				case ".tpt": contentType = "application/vnd.trid.tpt"; break;
				case ".mxs": contentType = "application/vnd.triscape.mxs"; break;
				case ".t": contentType = "text/troff"; break;
				case ".tra": contentType = "application/vnd.trueapp"; break;
				case ".ttf": contentType = "application/x-font-ttf"; break;
				case ".ttl": contentType = "text/turtle"; break;
				case ".umj": contentType = "application/vnd.umajin"; break;
				case ".uoml": contentType = "application/vnd.uoml+xml"; break;
				case ".unityweb": contentType = "application/vnd.unity"; break;
				case ".ufd": contentType = "application/vnd.ufdl"; break;
				case ".uri": contentType = "text/uri-list"; break;
				case ".utz": contentType = "application/vnd.uiq.theme"; break;
				case ".ustar": contentType = "application/x-ustar"; break;
				case ".uu": contentType = "text/x-uuencode"; break;
				case ".vcs": contentType = "text/x-vcalendar"; break;
				case ".vcf": contentType = "text/x-vcard"; break;
				case ".vcd": contentType = "application/x-cdlink"; break;
				case ".vsf": contentType = "application/vnd.vsf"; break;
				case ".wrl": contentType = "model/vrml"; break;
				case ".vcx": contentType = "application/vnd.vcx"; break;
				case ".mts": contentType = "model/vnd.mts"; break;
				case ".vtu": contentType = "model/vnd.vtu"; break;
				case ".vis": contentType = "application/vnd.visionary"; break;
				case ".viv": contentType = "video/vnd.vivo"; break;
				case ".ccxml": contentType = "application/ccxml+xml,"; break;
				case ".vxml": contentType = "application/voicexml+xml"; break;
				case ".src": contentType = "application/x-wais-source"; break;
				case ".wbxml": contentType = "application/vnd.wap.wbxml"; break;
				case ".wbmp": contentType = "image/vnd.wap.wbmp"; break;
				case ".wav": contentType = "audio/x-wav"; break;
				case ".davmount": contentType = "application/davmount+xml"; break;
				case ".woff": contentType = "application/x-font-woff"; break;
				case ".wspolicy": contentType = "application/wspolicy+xml"; break;
				case ".webp": contentType = "image/webp"; break;
				case ".wtb": contentType = "application/vnd.webturbo"; break;
				case ".wgt": contentType = "application/widget"; break;
				case ".hlp": contentType = "application/winhlp"; break;
				case ".wml": contentType = "text/vnd.wap.wml"; break;
				case ".wmls": contentType = "text/vnd.wap.wmlscript"; break;
				case ".wmlsc": contentType = "application/vnd.wap.wmlscriptc"; break;
				case ".wpd": contentType = "application/vnd.wordperfect"; break;
				case ".stf": contentType = "application/vnd.wt.stf"; break;
				case ".wsdl": contentType = "application/wsdl+xml"; break;
				case ".xbm": contentType = "image/x-xbitmap"; break;
				case ".xpm": contentType = "image/x-xpixmap"; break;
				case ".xwd": contentType = "image/x-xwindowdump"; break;
				case ".der": contentType = "application/x-x509-ca-cert"; break;
				case ".fig": contentType = "application/x-xfig"; break;
				case ".xhtml": contentType = "application/xhtml+xml"; break;
				case ".xml": contentType = "application/xml"; break;
				case ".xdf": contentType = "application/xcap-diff+xml"; break;
				case ".xenc": contentType = "application/xenc+xml"; break;
				case ".xer": contentType = "application/patch-ops-error+xml"; break;
				case ".rl": contentType = "application/resource-lists+xml"; break;
				case ".rs": contentType = "application/rls-services+xml"; break;
				case ".rld": contentType = "application/resource-lists-diff+xml"; break;
				case ".xslt": contentType = "application/xslt+xml"; break;
				case ".xop": contentType = "application/xop+xml"; break;
				case ".xpi": contentType = "application/x-xpinstall"; break;
				case ".xspf": contentType = "application/xspf+xml"; break;
				case ".xul": contentType = "application/vnd.mozilla.xul+xml"; break;
				case ".xyz": contentType = "chemical/x-xyz"; break;
				case ".yaml": contentType = "text/yaml"; break;
				case ".yang": contentType = "application/yang"; break;
				case ".yin": contentType = "application/yin+xml"; break;
				case ".zir": contentType = "application/vnd.zul"; break;
				case ".zip": contentType = "application/zip"; break;
				case ".zmm": contentType = "application/vnd.handheld-entertainment+xml"; break;
				case ".zaz": contentType = "application/vnd.zzazz.deck+xml"; break;
				default:
					contentType = "unknown/unknown";
					break;
			}

			return this.File(file.FileContents, contentType);
		}
	}
}