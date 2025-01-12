export interface IIngredientDB {
    ingredientID: number;
    naam: string;
}

export class IngredientDB implements IIngredientDB {
    constructor(ingredient: Ingredient) {
        this.ingredientID = ingredient.index;
        this.naam = ingredient.name.toLowerCase();
    }

    public ingredientID: number;
    public naam: string;
}

export class Ingredient {
    constructor(record: IIngredientDB) {
        if (record) {
            this.index = record.ingredientID;
            this.name = record.naam.toLowerCase();
        } else{
            this.index = 0;
            this.name = "";
        }
    }

    public index: number;
    public name: string;
}