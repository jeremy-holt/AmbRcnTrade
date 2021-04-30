import { Container } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import _ from "lodash";
import moment from "moment";
import "moment/locale/fr";
import "moment/locale/pt";
import "moment/locale/vi";
import { DATEFORMAT } from "../constants/app-constants";
import { IState } from "../store/state";
import { IEntity, IIdentity } from "./../core/interfaces/IEntity";
import { log } from "./log";

/**
 * Creates a delay
 * @param t time
 * @returns
 */
export const delay = (t: number) => {
  return new Promise(resolve => {
    setTimeout(resolve, t);
  });
};

export const randomInteger = (minimum: number, maximum: number) => (Math.floor(Math.random() * (maximum - minimum + 1)) + minimum);

export const hasId = (obj: IIdentity) => {
  if (obj?.id) {
    return !(obj.id === null || obj.id === undefined || obj.id === "");
  }
  return false;
};

export const isDate = (obj: unknown) => Object.prototype.toString.call(obj) === "[object Date]";

export const saveDiv = (a: number, b: number) => {
  return b > 0 ? a / b : 0;
};

// export const priceFormatter = (price: IPrice, decPlaces = 2) => {
//   if (price && price.currency !== undefined && price.value !== undefined) {
//     const currency = CurrencyEnum[price.currency].toUpperCase();
//     const value = numbro(price.value).format({ thousandSeparated: true, mantissa: decPlaces === undefined ? 2 : +decPlaces });

//     const result = `${currency.toUpperCase()} ${value}`;

//     return result;
//   }
//   return "";
// };

export const monthToMomentStringFormat = (month: number) => {
  const str = "00" + month.toString();
  return str.substring(str.length - 2);
};

export const trueFalse = (boolValue: string | boolean) => {
  if (boolValue === undefined || boolValue === null) {
    return undefined;
  }

  const value = boolValue !== undefined || boolValue !== null ? boolValue.toString() : undefined;

  if (value === undefined) {
    return undefined;
  }

  if (value && value.toUpperCase() === "TRUE") {
    return true;
  }

  if (!value || value.toUpperCase() === "FALSE") {
    return false;
  }

  return undefined;
};

export const decodeParams = (value: string | undefined | null) => {
  return value ? decodeURIComponent(value) : value;
};

export const encodeParams = (value: string | null | undefined) => {
  return value ? encodeURIComponent(value) : value;
};

export const fixAspNetCoreDate = (date: string | undefined | null, toToday: boolean) => {
  if (toToday) {
    return date ? moment(date).format(DATEFORMAT) : moment().format(DATEFORMAT);
  }

  return date ? moment(date).format(DATEFORMAT) : null;
};

export const toNumber = (value: string | number) => {
  if (value === undefined || value === null || value === "") {
    return 0;
  }
  return +value;
};

export const concatStringArray = (array: string[], seperator = ", ") => {
  if (!array) {
    return "";
  }
  const str = array.reduce((a, b) => a += b + seperator, "");
  return str.substr(0, str.length - seperator.length);
};

export const isDirty = <T>(state: IState, model: unknown, property: Fn<IState, T>, current?: Fn<T, unknown>): boolean => {
  const root = property(state);

  const base = current ? current(root) : (root as any).current;

  if (!_.isEqual(base, model)) {
    const differencesStateToModel = objectDifference(base, model, true);
    const differencesModelToState = objectDifference(model, state, true);

    const isEqual = differencesStateToModel === {} || differencesModelToState === {};

    if (!isEqual) {
      log.info("*** state and model are not equal ***");
      log.info("isEqual: differencesStateToModel", differencesStateToModel);
      log.info("isEqual: differencesModelToState", differencesModelToState);
    }
    return !isEqual;
  }
  return false;
};

export const captionNew = () => {
  const i18n = Container.instance.get(I18N);
  return i18n.tr("caption-new");
};

export const captionNewRow = () => {
  const i18n = Container.instance.get(I18N);
  return i18n.tr("caption-new-row");
};

export const cultureOrCompanyChanged = (newState: IState, oldState: IState) => {
  if (!oldState) {
    return false;
  }

  const hasChanged = (newState.culture !== oldState?.culture) || (newState.currentCompanyId !== oldState?.currentCompanyId);
  return hasChanged;
};

const isNullBlankOrUndefined = (o: unknown) => typeof o === "undefined" || o === null || o === "";

/*
* Deep diff between two object, using lodash
* @param  {Object} object Object compared
* @param  {Object} base   Object to compare with
* @param  {Object} ignoreBlanks will not include properties whose value is null, undefined, etc.
* @return {Object}        Return a new object who represent the diff
*/
export const objectDifference = (object: any, base: any, ignoreBlanks = false) => {
  if (!_.isObject(object) || _.isDate(object)) {
    return object;
  }            // special case dates

  return _.transform(object, (result: any, value: any, key: any) => {
    if (!_.isEqual(value, base[key])) {
      if (ignoreBlanks && isNullBlankOrUndefined(value) && isNullBlankOrUndefined(base[key])) { return; }
      result[key] = _.isObject(value) && _.isObject(base[key]) ? objectDifference(value, base[key]) : value;
    }
  });
};

export const capitalizeFirstLetter = (str: string) => {
  return str[0].toLocaleUpperCase() + str.substr(1);
};

export const hasErrorMessage = (state: IState) => {
  if (state && state.serverMessages) {
    return state.serverMessages.errorMessage?.length > 0;
  }
  return false;
};

export const copyHtmlTable = (tableId: string) => {
  let elToBeCopied: Node = undefined!;

  const node = document.querySelector(tableId);
  if (node) {
    elToBeCopied = node;
  } else {
    throw new Error(`Unable to find table with id ${tableId}`);
  }


  const range = document.createRange();
  const sel = document.getSelection();
  sel?.removeAllRanges();

  try {
    range.selectNodeContents(elToBeCopied);
    sel?.addRange(range);
  } catch (e) {
    range.selectNode(elToBeCopied);
    sel?.addRange(range);
  }

  document.execCommand("copy");
  sel?.removeAllRanges();
};

export const distinct = (list: IEntity[]) => {
  const result = [];
  const map = new Map();
  for (const item of list) {
    if (!map.has(item.id)) {
      map.set(item.id, true);
      result.push({
        id: item.id,
        name: item.name
      });
    }
  }
  return result;
};

export const distinctBy = <T, TU extends keyof T>(list: T[], key: TU) => {
  const result: { id: string | number, name: string }[] = [];
  const map = new Map();
  for (const item of list) {
    if (!map.has(item[key])) {
      map.set(item[key], true);
      let id: string | number = undefined!;      
      switch (typeof (item[key])) {
        case "string":
          id = item[key].toString();
          break;
        case "number":
          id = item[key] as unknown as number;
          break;
        default:
          throw new Error(`Cannot determine typeof ${item[key]}`);          
      }
      result.push({
        id: id,
        name: item["name"]
      });
    }
  }
  return result;
};

export const groupBy = ((list: unknown[], key: string | number) => {
  return list.reduce((rv, x) => {
    (rv[x[key]] = rv[x[key]] || []).push(x);
    return rv;
  }, {});
});

export const randomHtmlId = (prefix: string) => {
  return `${prefix.replace(" ", "_")}_${Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15)}`;
};

export const getRavenRootId =(id: string) =>{  
  return id?.split("/")[1].split("-")[0];
};

export type Lazy<T> = () => T;
export type Fn<T, TU> = (t: T) => TU;
export type Predicate = (e: unknown) => boolean;
// export type ById1<T> = { [key: string]: T }

// export interface IById<T> { [key: string]: T; }
export const head = <T>(array: T[]) => array[0];
export const tail = <T>(array: T[]) => array.slice(1);
export const ifElse = <T>(expr: boolean, t: Lazy<T>, f: Lazy<T>) => expr ? t() : f();
// export const act = <T, TU>(expr: Fn<T, TU>, value: unknown): TU => expr(value);

