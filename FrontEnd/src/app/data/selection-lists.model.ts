import { Ingredient } from "./ingredient.model";
import { Eenheid } from "./eenheid.model";

export class SelectionLists {
    constructor(public Eenheden: Eenheid[], public Ingredienten: Ingredient[]) { }
}