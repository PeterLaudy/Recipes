export interface IVerifyEmailData {
    Token: string;
    UserName: string;
}

export class VerifyEmailData implements IVerifyEmailData {
    constructor(token, username: string) {
        this.Token = token;
        this.UserName = username;
    }

    public Token: string;
    public UserName: string;
}