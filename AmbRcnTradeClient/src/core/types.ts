export type Lazy<T> = () => T;
export type Fn<T, U> = (t: T) => U;
export type Predicate = (e: unknown) => boolean;
// export type ById1<T> = { [key: string]: T }

// export interface IById<T> { [key: string]: T; }
export const head = <T>(array: T[]) => array[0];
export const tail = <T>(array: T[]) => array.slice(1);
export const ifElse = <T>(expr: boolean, t: Lazy<T>, f: Lazy<T>) => expr ? t() : f();
// export const act = <T, TU>(expr: Fn<T, TU>, value: unknown): TU => expr(value);
