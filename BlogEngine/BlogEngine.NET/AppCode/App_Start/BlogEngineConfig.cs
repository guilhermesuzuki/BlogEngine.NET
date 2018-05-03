using App_Code.Controls;
using BlogEngine.Core;
using BlogEngine.Core.Data;
using BlogEngine.Core.Data.Contracts;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Optimization;
using System.Web.UI;
using BlogEngine.NET.AppCode.Api;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System.Web.Routing;
using System.Web.Mvc;

namespace BlogEngine.NET.App_Start
{
	public class BlogEngineConfig
	{
		private static bool _initializedAlready = false;
		private readonly static object _SyncRoot = new Object();

		public static void Initialize(HttpContext context)
		{
			if (_initializedAlready) { return; }

			lock (_SyncRoot)
			{
				if (_initializedAlready) { return; }

				Utils.LoadExtensions();

				RegisterBundles(BundleTable.Bundles);

				RegisterRoutes(RouteTable.Routes);
				RegisterWebApi(GlobalConfiguration.Configuration);

				RegisterDiContainer();

				ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
					new ScriptResourceDefinition
					{
						Path = "~/Resources/jquery.min.js",
						DebugPath = "~/Resources/jquery.js",
						CdnPath = "http://ajax.microsoft.com/ajax/jQuery/jquery-3.3.1.min.js",
						CdnDebugPath = "http://ajax.microsoft.com/ajax/jQuery/jquery-3.3.1.js"
					});

				_initializedAlready = true;
			}
		}

		public static void SetCulture(object sender, EventArgs e)
		{
			var culture = BlogSettings.Instance.Culture;
			if (!string.IsNullOrEmpty(culture) && !culture.Equals("Auto"))
			{
				CultureInfo defaultCulture = Utils.GetDefaultCulture();
				Thread.CurrentThread.CurrentUICulture = defaultCulture;
				Thread.CurrentThread.CurrentCulture = defaultCulture;
			}
		}

		static void RegisterBundles(BundleCollection bundles)
		{
			BundleTable.EnableOptimizations = false;

			// new admin bundles
			bundles.IgnoreList.Clear();
			AddDefaultIgnorePatterns(bundles.IgnoreList);

			bundles.Add(
				new StyleBundle("~/(css,admincss)")
				.Include("~/Resources/[bootstrap]/css/bootstrap.min.css")
				.Include("~/Resources/[toastr]/toastr.min.css")
				.Include("~/Resources/[fontawesome]/css/fa-solid.min.css")
				.Include("~/Resources/[fontawesome]/css/fa-brands.min.css")
				.Include("~/Resources/[fontawesome]/css/fontawesome.css")
				.Include("~/Resources/[yoxview]/yoxview.css")
				.Include("~/Resources/star-rating.css")
				);

			//pure css style bundle
			bundles.Add(
				new StyleBundle("~/(css,pure)")
				.Include("~/Resources/[purecss]/pure-min.css")
				.Include("~/Resources/[purecss]/grids-responsive-min.css")
				.Include("~/Resources/pure-tinymce.css")
			);

			//common javascript
			bundles.Add(
				new ScriptBundle("~/(js,common)")
				.Include("~/Resources/jquery.min.js")
				.Include("~/Resources/jquery.migrate.min.js")
				.Include("~/Resources/jquery.cookie.min.js")
				.Include("~/Resources/jquery.jtemplates.min.js")
				.Include("~/Resources/json2.min.js")
				.Include("~/Resources/[bootstrap]/js/bootstrap.bundle.min.js")
				.Include("~/Resources/[yoxview]/yox.js")
				.Include("~/Resources/[yoxview]/jquery.yoxview-2.21.js")
				.Include("~/Resources/[tooltipster]/js/tooltipster.bundle.min.js")
				.Include("~/Resources/[toastr]/toastr.min.js")
				.Include("~/Resources/[syntaxhighlighter]/js/xregexp-min.js")
				.Include("~/Resources/[syntaxhighlighter]/js/shCore.js")
				.Include("~/Resources/[syntaxhighlighter]/js/shAutoloader.js")
				.Include("~/Resources/[syntaxhighlighter]/js/shActivator.js")
				.Include("~/Resources/[plyr]/plyr.js")
				.Include("~/Resources/blog.js")
				.Include("~/Resources/common.js")
				);

			//common css
			bundles.Add(
				new StyleBundle("~/(css,common)")
				.Include("~/Resources/site.css")
				.Include("~/Resources/star-rating.css")
				.Include("~/Resources/normalize.min.css")
				.Include("~/Resources/[bootstrap]/css/bootstrap.min.css")
				.Include("~/Resources/[fontawesome]/css/fa-solid.min.css")
				.Include("~/Resources/[fontawesome]/css/fa-brands.min.css")
				.Include("~/Resources/[fontawesome]/css/fontawesome.css")
				.Include("~/Resources/[purecss]/pure-min.css")
				.Include("~/Resources/[purecss]/grids-responsive-min.css")
				.Include("~/Resources/[yoxview]/yoxview.css")
				.Include("~/Resources/[tooltipster]/css/tooltipster.bundle.min.css")
				.Include("~/Resources/[toastr]/toastr.min.css")
				.Include("~/Resources/[plyr]/plyr.css")
				.Include("~/Resources/[syntaxhighlighter]/css/shCore.css")
				.Include("~/Resources/[syntaxhighlighter]/css/shCoreDefault.css")
				.Include("~/Resources/[syntaxhighlighter]/css/shThemeDefault.css")
			);

			//blogadmin javascript bundle
			bundles.Add(
				new ScriptBundle("~/(js,blogadmin)")
				.Include("~/Resources/jquery.js")
				.Include("~/Resources/jquery.migrate.min.js")
				.Include("~/Resources/jquery.form.min.js")
				.Include("~/Resources/jquery.validate.min.js")
				.Include("~/Resources/jquery.ui.min.js")
				.Include("~/Resources/[toastr]/toastr.min.js")
				.Include("~/Resources/[bootstrap]/js/bootstrap.bundle.min.js")
				.Include("~/Resources/[moment]/moment.min.js")
				.Include("~/Resources/[angular]/angular.min.js")
				.Include("~/Resources/[angular]/angular-route.min.js")
				.Include("~/Resources/[angular]/angular-sanitize.min.js")
				.Include("~/Resources/[angular]/angular-touch.min.js")
				.Include("~/admin/app/app.js")
				.Include("~/admin/app/listpager.js")
				.Include("~/admin/app/grid-helpers.js")
				.Include("~/admin/app/data-service.js")
				.Include("~/admin/app/editor/filemanagerController.js")
				.Include("~/admin/app/common.js")
				.Include("~/admin/app/dashboard/dashboardController.js")
				.Include("~/admin/app/content/blogs/blogController.js")
				.Include("~/admin/app/content/posts/postController.js")
				.Include("~/admin/app/content/pages/pageController.js")
				.Include("~/admin/app/content/tags/tagController.js")
				.Include("~/admin/app/content/categories/categoryController.js")
				.Include("~/admin/app/content/comments/commentController.js")
				.Include("~/admin/app/content/comments/commentFilters.js")
				.Include("~/admin/app/custom/plugins/pluginController.js")
				.Include("~/admin/app/custom/themes/themeController.js")
				.Include("~/admin/app/custom/widgets/widgetController.js")
				.Include("~/admin/app/custom/widgets/widgetGalleryController.js")
				.Include("~/admin/app/security/users/userController.js")
				.Include("~/admin/app/security/roles/roleController.js")
				.Include("~/admin/app/security/profile/profileController.js")
				.Include("~/admin/app/settings/settingController.js")
				.Include("~/admin/app/settings/tools/toolController.js")
				.Include("~/admin/app/settings/controls/blogrollController.js")
				.Include("~/admin/app/settings/controls/pingController.js")
				);

			bundles.Add(
				new ScriptBundle("~/(js,wysiwyg)")
				.Include("~/Resources/jquery.js")
				.Include("~/Resources/jquery.migrate.min.js")
				.Include("~/Resources/jquery.form.min.js")
				.Include("~/Resources/jquery.validate.min.js")
				.Include("~/Resources/jquery.textext.js")
				.Include("~/Resources/[toastr]/toastr.min.js")
				.Include("~/Resources/[angular]/angular.min.js")
				.Include("~/Resources/[angular]/angular-route.min.js")
				.Include("~/Resources/[angular]/angular-sanitize.min.js")
				.Include("~/Resources/[angular]/angular-touch.min.js")
				.Include("~/Resources/[bootstrap]/js/bootstrap.bundle.min.js")
				.Include("~/Resources/[moment]/moment.min.js")
				.Include("~/Resources/[tinymce]/editor.js")
				.Include("~/Resources/[tinymce]/tinymce.min.js")
				.Include("~/admin/app/app.js")
				.Include("~/admin/app/grid-helpers.js")
				.Include("~/admin/app/editor/editor-helpers.js")
				.Include("~/admin/app/editor/posteditorController.js")
				.Include("~/admin/app/editor/pageeditorController.js")
				.Include("~/admin/app/editor/filemanagerController.js")
				.Include("~/admin/app/common.js")
				.Include("~/admin/app/data-service.js")
				);
		}

		static void RegisterWebApi(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			config.Routes.MapHttpRoute("DefaultApiWithActionAndId", "api/{controller}/{action}/{id}");
			config.Filters.Add(new UnauthorizedAccessExceptionFilterAttribute());
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
			config.Services.Add(typeof(IExceptionLogger), new UnhandledExceptionLogger());
			//config.EnableSystemDiagnosticsTracing();
		}

		static void RegisterDiContainer()
		{
			var container = new Container();

			container.Register<ISettingsRepository, SettingsRepository>(Lifestyle.Transient);
			container.Register<IPostRepository, PostRepository>(Lifestyle.Transient);
			container.Register<IPageRepository, PageRepository>(Lifestyle.Transient);
			container.Register<IBlogRepository, BlogRepository>(Lifestyle.Transient);
			container.Register<IStatsRepository, StatsRepository>(Lifestyle.Transient);
			container.Register<IPackageRepository, PackageRepository>(Lifestyle.Transient);
			container.Register<ILookupsRepository, LookupsRepository>(Lifestyle.Transient);
			container.Register<ICommentsRepository, CommentsRepository>(Lifestyle.Transient);
			container.Register<ITrashRepository, TrashRepository>(Lifestyle.Transient);
			container.Register<ITagRepository, TagRepository>(Lifestyle.Transient);
			container.Register<ICategoryRepository, CategoryRepository>(Lifestyle.Transient);
			container.Register<ICustomFieldRepository, CustomFieldRepository>(Lifestyle.Transient);
			container.Register<IUsersRepository, UsersRepository>(Lifestyle.Transient);
			container.Register<IRolesRepository, RolesRepository>(Lifestyle.Transient);
			container.Register<IFileManagerRepository, FileManagerRepository>(Lifestyle.Transient);
			container.Register<ICommentFilterRepository, CommentFilterRepository>(Lifestyle.Transient);
			container.Register<IDashboardRepository, DashboardRepository>(Lifestyle.Transient);
			container.Register<IWidgetsRepository, WidgetsRepository>(Lifestyle.Transient);

			container.Verify();

			GlobalConfiguration.Configuration.DependencyResolver =
				new SimpleInjectorWebApiDependencyResolver(container);
		}

		static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
		{
			if (ignoreList == null)
				throw new ArgumentNullException("ignoreList");

			ignoreList.Ignore("*.intellisense.js");
			ignoreList.Ignore("*-vsdoc.js");

			//ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
			//ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
			//ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="routes"></param>
		static void RegisterRoutes(RouteCollection routes)
		{
			//ignore legacy routes from blogengine
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			//only lowercases
			routes.LowercaseUrls = true;

			/*files/media route*/
			routes.MapRoute(
				name: "files",
				url: "files/{*filepath}",
				defaults: new { controller = "Files", action = "Index", size = UrlParameter.Optional }
			);
		}
	}
}
