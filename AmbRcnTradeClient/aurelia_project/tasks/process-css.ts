import {CLIOptions, build} from "aurelia-cli";
import * as gulp from "gulp";
import * as project from "../aurelia.json";
import * as sass from "gulp-dart-sass";

export default function processCSS() {
  return gulp.src(project.cssProcessor.source, {sourcemaps: true})
    .pipe(CLIOptions.hasFlag("watch") ? sass.sync().on("error", sass.logError) : sass.sync())
    .pipe(build.bundle());
}

