export interface ICategorieDB {
    CategorieID: number;
    Naam: string;
    IconPath: string;
}

export class CategorieDB implements ICategorieDB {
    constructor(categorie: Categorie) {
        this.CategorieID = categorie.categorieID;
        this.Naam = categorie.naam;
        this.IconPath = categorie.iconPath;
    }

    CategorieID: number;
    Naam: string;
    IconPath: string;
}

export class Categorie {
    constructor(record: ICategorieDB) {
        if (record) {
            this.categorieID = record.CategorieID;
            this.naam = record.Naam;
            this.iconPath = record.IconPath;
        } else {
            this.categorieID = 0;
            this.naam = "";
            this.iconPath = "";
        }
    }

    categorieID: number;
    naam: string;
    iconPath: string;
}