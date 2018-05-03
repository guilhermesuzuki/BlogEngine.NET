/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    //sass = require("gulp-sass"),
    rename = require("gulp-rename");

gulp.task("resources", function () {

    //bootstrap
    gulp.src(["bower_components/bootstrap-4.0.2/**/*.min.*"])
        .pipe(gulp.dest("Resources/[bootstrap]"));

    //purecss
    gulp.src(["bower_components/pure/*-min.*"])
        .pipe(gulp.dest("Resources/[purecss]"));

    //normalize
    gulp.src(["bower_components/normalize-css/normalize.css"])
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest("Resources/"));

    //jquery
    gulp.src([
        "bower_components/jquery/dist/jquery.min.*",
        "bower_components/jquery/dist/jquery.js",
        "bower_components/jquery-validation/dist/jquery.validate.min.*",
        "bower_components/jquery-form/dist/*.min.*",])
        .pipe(gulp.dest("Resources/"));

    //jquery cookie, json2
    gulp.src(["bower_components/jquery-cookie/jquery.cookie.js", "bower_components/json2-js/json2.js",])
        .pipe(uglify())
        .pipe(rename({ suffix: ".min", extname: ".js" }))
        .pipe(gulp.dest("Resources/"));

    //jquery-ui
    gulp.src(["bower_components/jquery-ui/*.min.*"])
        .pipe(rename({ basename: "jquery.ui", suffix: ".min" }))
        .pipe(gulp.dest("Resources/"));

    //jquery-migrate
    gulp.src(["bower_components/jquery-migrate-301/*.min.js",])
        .pipe(rename({ basename: "jquery.migrate", suffix: ".min", extname: ".js" }))
        .pipe(gulp.dest("Resources/"));

    //angular (with route, animate)
    gulp.src([
        "bower_components/angular/angular.min.*",
        "bower_components/angular-animate/angular-*.min.*",
        "bower_components/angular-route/angular-*.min.*",
        "bower_components/angular-sanitize/angular-*.min.*",
        "bower_components/angular-touch/angular-*.min.*"])
        .pipe(gulp.dest("Resources/[angular]"));

    //fontawesome
    gulp.src([
        "bower_components/font-awesome/web-fonts-with-css/css/fontawesome-all.css",
        "bower_components/font-awesome/web-fonts-with-css/css/fontawesome.css",
        "bower_components/font-awesome/web-fonts-with-css/css/fa-*.css"])
        .pipe(gulp.dest("Resources/[fontawesome]/css"));

    gulp.src(["bower_components/font-awesome/web-fonts-with-css/webfonts/*.*",])
        .pipe(gulp.dest("Resources/[fontawesome]/webfonts"));

    //toastr
    gulp.src(["bower_components/toastr/*.min.js", "bower_components/toastr/*.min.css"])
        .pipe(gulp.dest("Resources/[toastr]"));

    //moment
    gulp.src(["bower_components/moment/min/*.min.js"])
        .pipe(gulp.dest("Resources/[moment]"));

    //tooltipster
    gulp.src([
        "bower_components/tooltipster/dist/**/*.min.js",
        "bower_components/tooltipster/dist/**/*.min.css"])
        .pipe(gulp.dest("Resources/[tooltipster]"));

    //tinymce
    gulp.src([
        "bower_components/tinymce/**/*.min.js",
        "bower_components/tinymce/**/*.min.css"])
        .pipe(gulp.dest("Resources/[tinymce]"));

    //syntaxhighlighter
    gulp.src(["bower_components/SyntaxHighlighter/src/js/*.js", "bower_components/xregexp/min/xregexp-min.js"]).pipe(gulp.dest("Resources/[syntaxhighlighter]/js"));
    gulp.src(["bower_components/SyntaxHighlighter/src/sass/*.scss"]).pipe(gulp.dest("Resources/[syntaxhighlighter]/scss"));

    //tinymce
    gulp.src(["bower_components/plyr/dist/plyr.*",])
        .pipe(gulp.dest("Resources/[plyr]"));

    /*gulp.src(["bower_components/SyntaxHighlighter/src/sass/shCore*.scss"])
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest("Resources/[syntaxhighlighter]/css"));*/

});