export interface ICategorieDB {
    CategorieID: number;
    Naam: string;
    IconIndex: number;
}

export class CategorieDB implements ICategorieDB {
    constructor(categorie: Categorie) {
        this.CategorieID = categorie.categorieID;
        this.Naam = categorie.naam;
        this.IconIndex = categorie.iconIndex;
    }

    CategorieID: number;
    Naam: string;
    IconIndex: number;
}

export class Categorie {
    constructor(record: ICategorieDB) {
        if (record) {
            this.categorieID = record.CategorieID;
            this.naam = record.Naam;
            this.iconIndex = record.IconIndex;
        } else {
            this.categorieID = 0;
            this.naam = "";
            this.iconIndex = -1;
        }
    }

    categorieID: number;
    naam: string;
    iconIndex: number;
}