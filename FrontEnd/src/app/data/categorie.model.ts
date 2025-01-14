export interface ICategorieDB {
    categorieID: number;
    naam: string;
    iconPath: string;
}

export class CategorieDB implements ICategorieDB {
    constructor(categorie: Categorie) {
        this.categorieID = categorie.index;
        this.naam = categorie.name;
    }

    categorieID: number;
    naam: string;
    iconPath: string;
}

export class Categorie {
    constructor(record: ICategorieDB) {
        if (record) {
            this.index = record.categorieID;
            this.name = record.naam;
            this.iconPath = record.iconPath;
        } else {
            this.index = 0;
            this.name = "";
            this.iconPath = "";
        }
    }

    index: number;
    name: string;
    iconPath: string;
}