export class CustomError extends Error {
    constructor(public readonly errorCode: number, ...params: any[]) {
        super(...params);

        if (Error.captureStackTrace) {
            Error.captureStackTrace(this, CustomError);
        }
    }
}
