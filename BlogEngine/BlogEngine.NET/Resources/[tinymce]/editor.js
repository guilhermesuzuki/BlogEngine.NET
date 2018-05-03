var editorGetHtml = function () {
    return tinymce.activeEditor.getContent();
}

var editorSetHtml = function (html) {
    window.setTimeout(
        function () {
            tinymce.activeEditor.setContent(html);
        },
        1000
    );
}

$(document).ready(
    function () {
        tinymce.init({
            selector: '#txtContent',
            plugins: [
                "advlist autolink lists link image charmap print preview anchor",
                "searchreplace visualblocks code fullscreen textcolor imagetools",
                "insertdatetime media table contextmenu paste sh4tinymce filemanager",
                "emoticons",
            ],
            toolbar: "styleselect | bold underline italic | alignleft aligncenter alignright | bullist numlist | forecolor backcolor | link media sh4tinymce emoticons | fullscreen code | filemanager",
            contextmenu: "link image inserttable | cell row column deletetable",
            autosave_ask_before_unload: false,
            max_height: 1000,
            min_height: 300,
            height: 500,
            menubar: false,
            relative_urls: false,
            browser_spellcheck: true,
            paste_data_images: true,
            setup: function (editor) {
                editor.on('init', function (e) {
                    // (adds pure css) DO NOT TAKE THIS OFF (TINYMCE USES AN IFRAME FOR THE EDITOR ELEMENTS, SO ADDING STYLES OFF THIS METHOD WON'T WORK
                    var pure = editor.getDoc().createElement("link");
                    var href = editor.getDoc().createAttribute("href");
                    href.value = "/(css,pure)?v=" + Math.random();
                    var rel = editor.getDoc().createAttribute("rel");
                    rel.value = "stylesheet";

                    pure.setAttributeNode(href);
                    pure.setAttributeNode(rel);

                    var head = editor.getDoc().getElementsByTagName("head")[0];
                    head.appendChild(pure);
                });
            },
        });
    });
