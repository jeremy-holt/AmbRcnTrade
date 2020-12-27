import { Fn } from "./types";

export const sum = <T>(array: T[], fieldPath?: Fn<T, number>): number => {
  if (!fieldPath && typeof +array[0] !== "number") {
    throw new Error("The array is of type object, so you must provide a path");
  }

  const total = fieldPath
    ? array.reduce((t, val) => t += +fieldPath(val) || 0, 0)
    : array.reduce((t, val) => t += (+val as unknown) as number || 0, 0);

  return total;
};

export const sumArrays = <T>(arrays: T[][], fieldPath?: Fn<T, number>): number[] => {
  const result: number[] = Array(arrays[0].length).fill(0);

  if (!arrays.every(c => c.length === result.length)) {
    throw new Error("The arrays must all be of the same size");
  }

  if (!fieldPath && typeof arrays[0][0] !== "number") {
    throw new Error("The arrays are of type object, so you must provide a path");
  }

  arrays.forEach(outerArray => {
    if (fieldPath) {
      outerArray.forEach((item, idx) => {
        result[idx] += +fieldPath(item);
      });
    } else {
      outerArray.forEach((val, idx) => {
        result[idx] += (+val as unknown) as number;
      });
    }
  });

  return result as number[];
};

export const arrayAdder = (...arrays: number[][]): number[] => {
  const numArrays = arrays.length;
  const length = arrays[0].length;
  const result: number[] = Array(length).fill(0);

  if (arrays.some(c => c.length !== length)) {
    throw Error("The arrays must all be of the same length");
  }

  if (numArrays < 2) {
    throw Error("Please provide at least two arrays to add together");
  }

  for (let x = 0; x < numArrays; x++) {
    for (let y = 0; y < length; y++) {
      result[y] = result[y] + arrays[x][y];
    }
  }
  return result;
};

export const arraySubtracter = (first: number[], second: number[]): number[] => {
  const length = first.length;

  if (length !== second.length) {
    throw Error("The arrays must both be of the same length");
  }

  const result = Array(length).fill(0);

  for (let i = 0; i < length; i++) {
    result[i] = first[i] - second[i];
  }
  return result;
};

export const arrayAccumulator = (array: number[]): number[] => {
  const length = array.length;
  const result = Array(length).fill(0);

  for (let i = 0; i < length; i++) {
    const prev = i === 0 ? 0 : result[i - 1];
    result[i] = prev + array[i];
  }
  return result;
};

export const arrayMaxValue = (array: number[]): number => {
  return Math.max(...array);
};

export const arrayMinValue = (array: number[]): number => {
  return Math.min(...array);
};

export const concatArray = <T, U>(array: T[], fieldPath: Fn<T, U>, eol: string = ", ") => {
  let result = "";

  const lastItem = array[array.length - 1];

  array.forEach(c => {
    result += fieldPath(c);
    if (c !== lastItem) {
      result += eol;
    }
  });
  return result;
};

export const intersect = <T>(a: T[], b: T[]) => {
  if (!a || !b) {
    return [];
  }
  return a.filter(x => !b.includes(x));
};

export const except = <T>(a: T[], b: T[]) => {
  return a.filter(x => !b.includes(x)).concat(b.filter(x => !a.includes(x)));
};

export const union = <T>(a: T[], b: T[]) => {
  return Array.from(new Set([...a, ...b]));
};

export const range = (start: number, end?: number, step: number = 1) => {
  const list: number[] = [];
  if (!end) {
    end = start;
    start = 0;
  } else {
    end++;
  }

  if (end < start) {
    throw new Error("Start must be less than end");
  }

  for (let i = start; i < end; i += step) {
    list.push(i);
  }

  return list;
};
