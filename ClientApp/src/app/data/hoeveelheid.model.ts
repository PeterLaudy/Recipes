import { IngredientDB, Ingredient } from "./ingredient.model";
import { EenheidDB, Eenheid } from "./eenheid.model";

export interface IHoeveelheidDB {
    hoeveelheidID: number;
    gerechtID: number;
    ingredientID: number;
    eenheidID: number;
    aantal: number;
    ingredient: IngredientDB;
    eenheid: EenheidDB;
}

export class HoeveelheidDB implements IHoeveelheidDB {
    constructor(hoeveelheid: Hoeveelheid) {
        this.hoeveelheidID = hoeveelheid.index;
        this.gerechtID = hoeveelheid.gerechtIndex;
        this.ingredientID = hoeveelheid.ingredientIndex;
        this.eenheidID = hoeveelheid.eenheidIndex;
        this.aantal = hoeveelheid.aantal;
        this.ingredient = new IngredientDB(hoeveelheid.Ingredient);
        this.eenheid = new EenheidDB(hoeveelheid.Eenheid);
}

    public hoeveelheidID: number;
    public gerechtID: number;
    public ingredientID: number;
    public eenheidID: number;
    public aantal: number;
    public ingredient: IngredientDB;
    public eenheid: EenheidDB;
}

export class Hoeveelheid {
    constructor(record: IHoeveelheidDB) {
        if (record) {
            this.index = record.hoeveelheidID;
            this.gerechtIndex = record.gerechtID;
            this.ingredientIndex = record.ingredientID;
            this.eenheidIndex = record.eenheidID;
            this.aantal = record.aantal;
            this.Ingredient = new Ingredient(record.ingredient);
            this.Eenheid = new Eenheid(record.eenheid);
        } else {
            this.index = 0;
            this.gerechtIndex = 0;
            this.ingredientIndex = 0;
            this.eenheidIndex = 0;
            this.aantal = 0;
            this.Ingredient = new Ingredient(null);
            this.Eenheid = new Eenheid(null);
        }
    }

    public index: number;
    public gerechtIndex: number;
    public ingredientIndex: number;
    public eenheidIndex: number;
    public aantal: number;
    public Ingredient: Ingredient;
    public Eenheid: Eenheid;
}