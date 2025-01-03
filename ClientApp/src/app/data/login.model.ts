export interface ILoginData {
    UserName: string;
    Password: string;
}

export class LoginData implements ILoginData {
    constructor(name: string, password: string) {
        this.UserName = name;
        this.Password = password;
    }

    public UserName: string;
    public Password: string;
}