import { Photo } from './photo';

export interface User {
    id: number;
    username: string;
    userpassword: string;
    knownAs: string;
    age: number;
    gender: string;
    createdMyProperty: Date;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    interest?: string; // optional
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];

}
