import { autoinject } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { Store } from "aurelia-store";
import { BindingSignaler } from "aurelia-templating-resources";
import _ from "lodash";
import moment from "moment";
import "moment/locale/fr";
import "moment/locale/pt";
import "moment/locale/vi";
import { LOCAL_STORAGE } from "../localStorage-consts";
import { IState } from "../store/state";

@autoinject
export class LanguageService {

  constructor(
    private readonly i18n: I18N,
    private readonly signaller: BindingSignaler,
    private store: Store<IState>) {

    store.registerAction("languageAction", languageAction);

    moment.locale("en");
  }

  public async setCulture(culture: string) {
    await this.i18n.setLocale(culture);
    moment.locale(culture);
    this.signaller.signal("aurelia-translation-signal");

    this.store.dispatch(languageAction, culture);
  }

  public static get culture() {
    const culture = localStorage.getItem(LOCAL_STORAGE.culture);
    return culture || "en";
  }

  public static get cultureFull() {
    const culture = localStorage.getItem(LOCAL_STORAGE.culture);
    switch (culture) {
      case "fr": return "fr-FR";
      case "pt": return "pt-BR";
      default: return "en-GB";
    }
  }
}

export function languageAction(state: IState, culture: string) {
  const newState = _.cloneDeep(state);
  newState.culture = culture;
  localStorage.setItem(LOCAL_STORAGE.culture, culture);
  return newState;
}

