export class ChangePasswordData {
    constructor(public Token, public UserName, public Password, public ConfirmPassword: string) {
    }
}