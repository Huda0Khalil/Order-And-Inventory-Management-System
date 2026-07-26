import { ICategory } from "./ICategory";
import { ISupplier } from "./ISupplier";

export interface IProduct {
  Id: number;
  Name: string;
  Price: number;
  Barcode: string;
  StockQuantity: number;
  Category?: ICategory;
  CategoryId: number;
  SupplierId: number;
  supplier?:ISupplier;
  TenantId:string;
}