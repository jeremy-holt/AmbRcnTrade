import { initialState } from "./store/initial-state";
import { Aurelia } from "aurelia-framework";
import environment from "./environment";
import "bootstrap";
import * as process from "process";

export function configure(aurelia: Aurelia) {

  const toolbarOptions = [
    ["bold", "italic", "underline", "strike"],        // toggled buttons
    ["blockquote", "code-block"],

    [{ "header": 1 }, { "header": 2 }],               // custom button values
    [{ "list": "ordered" }, { "list": "bullet" }],
    [{ "script": "sub" }, { "script": "super" }],      // superscript/subscript
    [{ "indent": "-1" }, { "indent": "+1" }],          // outdent/indent
    [{ "direction": "rtl" }],                         // text direction

    [{ "size": ["small", false, "large", "huge"] }],  // custom dropdown
    [{ "header": [1, 2, 3, 4, 5, 6, false] }],

    [{ "color": [] }, { "background": [] }],          // dropdown with defaults from theme
    [{ "font": [] }],
    [{ "align": [] }],

    ["clean"]                                         // remove formatting button
  ];
  aurelia.use
    .standardConfiguration()
    .plugin("aurelia-dialog")
    .plugin("aurelia-validation")
    .plugin("aurelia-animator-css")
    .plugin("bcx-aurelia-reorderable-repeat")
    .plugin("aurelia-api", (config: { registerEndpoint: (arg0: string) => void; }) => {
      config.registerEndpoint("auth");
    })
    .plugin("aurelia-store", {
      initialState,
      devToolsOptions: { serialize: false }
    })
    .plugin("aurelia-authentication")    
    .plugin("aurelia-quill-plugin", { modules: { toolbar: toolbarOptions } })
    .feature("resources");

  aurelia.use.developmentLogging(environment.debug ? "debug" : "warn");

  if (environment.testing) {
    aurelia.use.plugin("aurelia-testing");
  }

  window.process = process;

  return aurelia.start().then(() => aurelia.setRoot());
}
