INSERT INTO "CatalogType" ("Id","Type") VALUES 
(1,'Mug')
,(2,'T-Shirt')
,(3,'Sheet')
,(4,'USB Memory Stick')
;

INSERT INTO "CatalogBrand" ("Id","Brand") VALUES 
(1,'Azure')
,(2,'.NET')
,(3,'Visual Studio')
,(4,'SQL Server')
,(5,'Other')
;

INSERT INTO "Catalog" ("Id","Name","Description","Price","PictureFileName","CatalogTypeId","CatalogBrandId","AvailableStock","RestockThreshold","MaxStockThreshold","OnReorder") VALUES 
(1,'.NET Bot Black Hoodie','.NET Bot Black Hoodie, and more',19.50,'1.png',2,2,100,0,200,False)
,(2,'.NET Black & White Mug','.NET Black & White Mug',8.50,'2.png',1,3,89,0,200,True)
,(3,'Prism White T-Shirt','Prism White T-Shirt',12.00,'3.png',2,5,56,0,200,False)
,(4,'.NET Foundation T-shirt','.NET Foundation T-shirt',12.00,'4.png',2,2,120,0,200,False)
,(5,'Roslyn Red Sheet','Roslyn Red Sheet',8.50,'5.png',3,5,55,0,200,False)
,(6,'.NET Blue Hoodie','.NET Blue Hoodie',12.00,'6.png',2,2,17,0,200,False)
,(7,'Roslyn Red T-Shirt','Roslyn Red T-Shirt',12.00,'7.png',2,5,8,0,200,False)
,(8,'Kudu Purple Hoodie','Kudu Purple Hoodie',8.50,'8.png',2,5,34,0,200,False)
,(9,'Cup<T> White Mug','Cup<T> White Mug',12.00,'9.png',1,5,76,0,200,False)
,(10,'.NET Foundation Sheet','.NET Foundation Sheet',12.00,'10.png',3,2,11,0,200,False)
,(11,'Cup<T> Sheet','Cup<T> Sheet',8.50,'11.png',3,2,3,0,200,False)
,(12,'Prism White TShirt','Prism White TShirt',12.00,'12.png',2,5,0,0,200,False)
;