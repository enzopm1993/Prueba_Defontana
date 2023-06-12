# Prueba_Defontana
Prueba realizada con la version de .NET 6.0  

## Especificaciones
Instalación de los paquetes Microsoft.EntityFrameworkCore.SqlServer 
Instalación de los paquetes Microsoft.EntityFrameworkCore.Tools
Ejecucion del comando Scaffold-DbContext para la generación automática de las entidades y el contexto.

## Funcionamiento
Ejecutar la aplicación de Consola y se mostrará el resultado de cada pregunta.
![captura1](/captura/1.png "captura1")

## Consultas SQL
```sql
-- En DATEADD se especifico -31 para que el conteo de dias hacia atras lo haga desde el ultimo día
```

```sql
-- El total de ventas de los últimos 30 días (monto total y cantidad total de ventas)
SELECT SUM(Total) AS MontoTotalVentas, COUNT(*) AS CantidadTotalVentas
FROM Venta
WHERE Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE));
```


```sql
--El día y hora en que se realizó la venta con el monto más alto (y cuál es aquel monto)
SELECT TOP 1 Fecha, Total
FROM Venta
WHERE Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE))
ORDER BY Total DESC;
```


```sql
--Indicar cuál es el producto con mayor monto total de ventas
SELECT TOP 1 VD.ID_Producto, P.Nombre AS Producto, SUM(VD.TotalLinea) AS MontoTotalVentas
FROM Venta V INNER JOIN VentaDetalle VD
ON VD.ID_Venta = V.ID_Venta LEFT JOIN Producto P
ON P.ID_Producto = VD.ID_Producto
WHERE Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE))
GROUP BY VD.ID_Producto, P.Nombre
ORDER BY MontoTotalVentas DESC;
```


```sql
--Indicar el local con mayor monto de ventas
SELECT TOP 1 V.ID_Local, L.Nombre AS Local, SUM(Total) AS MontoTotalVentas
FROM Venta V LEFT JOIN Local L
ON L.ID_Local = V.ID_Local
WHERE Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE))
GROUP BY V.ID_Local, L.Nombre
ORDER BY MontoTotalVentas DESC;
```


```sql
--¿Cuál es la marca con mayor margen de ganancias?
SELECT TOP 1 P.ID_Marca, M.Nombre as Marca, SUM((VD.TotalLinea - (P.Costo_Unitario * VD.Cantidad))) AS MargenGanancias
FROM Venta V INNER JOIN VentaDetalle VD
ON V.ID_Venta = VD.ID_Venta LEFT JOIN Producto P
ON P.ID_Producto = VD.ID_Producto LEFT JOIN Marca M
ON M.ID_Marca = P.ID_Marca
WHERE V.Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE))
GROUP BY p.ID_Marca, M.Nombre 
ORDER BY MargenGanancias DESC;));
```

```sql
--¿Cómo obtendrías cuál es el producto que más se vende en cada local?
SELECT
    L.ID_Local,
	L.Nombre AS Local,
    VD.ID_Producto,
	VD.Producto,
    VD.Cantidad_Total_Vendida
FROM
    (
        SELECT
            V.ID_Local,
            VD.ID_Producto,
			P.Nombre AS Producto,
            SUM(VD.Cantidad) AS Cantidad_Total_Vendida,
            MAX(SUM(VD.Cantidad)) OVER (PARTITION BY V.ID_Local) AS MaxCantidad
        FROM VentaDetalle VD INNER JOIN Venta V 
		ON VD.ID_Venta = V.ID_Venta LEFT JOIN Producto P
		ON P.ID_Producto = VD.ID_Producto
		WHERE V.Fecha >= DATEADD(day, -31, CAST(GETDATE() AS DATE))
        GROUP BY
            V.ID_Local, VD.ID_Producto, P.Nombre
    ) AS VD LEFT JOIN Local L 
	ON L.ID_Local = VD.ID_Local
WHERE VD.Cantidad_Total_Vendida = VD.MaxCantidad;
```

