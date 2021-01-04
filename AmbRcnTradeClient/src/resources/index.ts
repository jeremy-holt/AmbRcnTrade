import {FrameworkConfiguration} from "aurelia-framework";

export function configure(config: FrameworkConfiguration) {
  config.globalResources([
    "./attributes/link-disabled",
    "./elements/toolbar/toolbar",
    "./elements/toolbar/toolbar-item",
    "./elements/sidenav/sidenav",
    "./elements/progress-bar/progress-bar",
    "./elements/text-input/text-input",
    "./elements/text-area-input/text-area-input",
    "./elements/email-input/email-input",
    "./elements/select-input/select-input",
    "./elements/date-input/date-input",
    "./elements/number-input/number-input",
    "./elements/number-input-suffix/number-input-suffix",
    "./elements/password-input/password-input",
    "./elements/switch/switch",
    "./elements/toast/toast",
    "./elements/checkbox-input/checkbox-input",
    "./elements/debug-console/debug-console",
    "./elements/image-viewer/image-viewer",
    "./elements/pdf-viewer/pdf-viewer",
    "./elements/error-alert/error-alert",
    "./elements/help-toolbar-link/help-toolbaritem-link",
    "./elements/file-uploader/file-uploader",
    "./value-converters/date-format",
    "./value-converters/number-format",
    "./value-converters/pos-neg-color",
    "./value-converters/toJson",
    "./value-converters/number",
    "./value-converters/currency-format",
    "./value-converters/customer-filter"
  ]);
}
