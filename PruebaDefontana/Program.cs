using PruebaDefontana.Models;

namespace TuProyecto
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new PruebaContext())
            {
                DateTime fecha = DateTime.Now.AddDays(-30).Date; //Filtro solo por fecha, sin hora. Obtengo la fecha de los ultimos 30 dias.

                var ventas = (from v in context.Venta
                                join vd in context.VentaDetalles on v.IdVenta equals vd.IdVenta
                                join p in context.Productos on vd.IdProducto equals p.IdProducto into prodGroup
                                from prod in prodGroup.DefaultIfEmpty()
                                join m in context.Marcas on prod.IdMarca equals m.IdMarca into marcaGroup
                                from marca in marcaGroup.DefaultIfEmpty()
                                join l in context.Locals on v.IdLocal equals l.IdLocal into localGroup
                                from local in localGroup.DefaultIfEmpty()
                                where v.Fecha >= fecha
                                select new
                                {
                                    v.IdVenta,
                                    v.IdLocal,
                                    Local = local.Nombre,
                                    v.Fecha,
                                    v.Total,
                                    vd.IdVentaDetalle,
                                    vd.IdProducto,
                                    Producto = prod.Nombre,
                                    prod.CostoUnitario,
                                    vd.Cantidad,
                                    vd.PrecioUnitario,
                                    vd.TotalLinea,
                                    ID_Marca = prod.IdMarca,
                                    Marca = marca.Nombre
                                }).ToList();

               
                // El total de ventas de los últimos 30 días (monto total y cantidad total de ventas)
                var ventasTotales30Dias = from vt in ventas
                                      group vt by vt.IdVenta into g
                                      select new
                                      {
                                          IdVenta = g.Key,
                                          Total = g.FirstOrDefault()?.Total
                                      };

                var montoTotalVentas = ventasTotales30Dias.Sum(v => v.Total);
                var cantidadTotalVentas = ventasTotales30Dias.Count();

                // El día y hora en que se realizó la venta con el monto más alto (y cuál es aquel monto)
                var ventaMontoMasAltoFecha = from vt in ventas
                                          group vt by vt.IdVenta into g
                                          select new
                                          {
                                              IdVenta = g.Key,
                                              Fecha = g.FirstOrDefault()?.Fecha,
                                              Total = g.FirstOrDefault()?.Total
                                          };


                var ventaMasAlta = ventaMontoMasAltoFecha.OrderByDescending(v => v.Total).FirstOrDefault();
                var diaHoraVentaMasAlta = ventaMasAlta.Fecha;
                var montoVentaMasAlta = ventaMasAlta.Total;

                // Indicar cuál es el producto con mayor monto total de ventas
                var productoMayorMonto= from vt in ventas
                                             group vt by new { vt.IdProducto, vt.Producto } into g
                                             select new
                                             {
                                                 IdProducto = g.Key.IdProducto,
                                                 Producto = g.Key.Producto,
                                                 MontoTotalVentas = g.Sum(v => v.TotalLinea)
                                             };

                var productoMayorMontoVentas = productoMayorMonto.OrderByDescending(v => v.MontoTotalVentas).FirstOrDefault();
                var productoIDMayorMontoVentas = productoMayorMontoVentas.IdProducto;
                var productoNombreMayorMontoVentas = productoMayorMontoVentas.Producto;
                var montoTotalVentasProducto = productoMayorMontoVentas.MontoTotalVentas;

                // Indicar el local con mayor monto de ventas
                var grupoLocalVentaMayorMonto = from vt in ventas
                                         group vt by new { vt.IdLocal, vt.Local, vt.IdVenta } into g
                                         select new
                                         {
                                             IdLocal = g.Key.IdLocal,
                                             Local = g.Key.Local,
                                             Total = g.FirstOrDefault()?.Total
                                         };


                var localMayorMonto = from vt in grupoLocalVentaMayorMonto
                                      group vt by new { vt.IdLocal, vt.Local } into g
                                      select new
                                      {
                                          IdLocal = g.Key.IdLocal,
                                          Local = g.Key.Local,
                                          MontoTotalVentas = g.Sum(v => v.Total)
                                      };

                var localMayorMontoVentas = localMayorMonto.OrderByDescending(v => v.MontoTotalVentas).FirstOrDefault();
                var localIDMayorMontoVentas = localMayorMontoVentas.IdLocal;
                var localNombreMayorMontoVentas = localMayorMontoVentas.Local;
                var montoTotalVentasLocal = localMayorMontoVentas.MontoTotalVentas;

                // ¿Cuál es la marca con mayor margen de ganancias?
                var marcaMayorMargen = from vt in ventas
                                                group vt by new { vt.ID_Marca, vt.Marca } into g
                                                select new
                                                {
                                                    IdMarca = g.Key.ID_Marca,
                                                    Marca = g.Key.Marca,
                                                    MargenGanancias = g.Sum(v => v.TotalLinea - (v.CostoUnitario * v.Cantidad))
                                                };

                var marcaMayorMargenGanancias = marcaMayorMargen.OrderByDescending(v => v.MargenGanancias).FirstOrDefault();
                var marcaIDMayorMargenGanancias = marcaMayorMargenGanancias.IdMarca;
                var marcaNombreMayorMargenGanancias = marcaMayorMargenGanancias.Marca;
                var margenGananciasMarca = marcaMayorMargenGanancias.MargenGanancias;

                // ¿Cómo obtendrías cuál es el producto que más se vende en cada local?
                var productoMasVendidoPorLocal = ( from v in ventas
                                                group v by new { v.IdLocal, v.IdProducto } into g
                                                let Cantidad_Total_Vendida = g.Sum(v => v.Cantidad)
                                                select new
                                                {
                                                    IdLocal = g.Key.IdLocal,
                                                    Local = g.FirstOrDefault()?.Local,
                                                    IdProducto = g.Key.IdProducto,
                                                    Producto = g.FirstOrDefault()?.Producto,
                                                    Cantidad_Total_Vendida
                                                })
                                                .GroupBy(g => g.IdLocal)
                                                .SelectMany(g => g.Where(p => p.Cantidad_Total_Vendida == g.Max(p => p.Cantidad_Total_Vendida)))
                                                .OrderBy(o => o.IdLocal)
                                                .ToList();


                

                Console.WriteLine($"Monto y Cantidad total de ventas en los últimos 30 días: Monto = {montoTotalVentas}, Cantidad = {cantidadTotalVentas} ");
                Console.WriteLine($"Día, hora y Monto en que se realizó la venta con el monto  mas alto: Fecha = {diaHoraVentaMasAlta}, Monto = {montoVentaMasAlta} ");
                Console.WriteLine($"Producto con mayor monto total de ventas: IdProducto = {productoIDMayorMontoVentas}, Producto = {productoNombreMayorMontoVentas}, Monto = {montoTotalVentasProducto} ");
                Console.WriteLine($"Local con mayor monto de ventas: IdLocal = {localIDMayorMontoVentas}, Local = {localNombreMayorMontoVentas}, Monto = {montoTotalVentasLocal} ");
                Console.WriteLine($"Marca con mayor margen de ganancias: IdMarca = {marcaIDMayorMargenGanancias}, Marca = {marcaNombreMayorMargenGanancias}, Margen = {margenGananciasMarca} ");
                
                
                Console.WriteLine($"Producto que más se vende en cada local: ");

                foreach(var p in productoMasVendidoPorLocal)
                {
                    Console.WriteLine($"IdLocal = {p.IdLocal}, Local = {p.Local}, IdProducto = {p.IdProducto}, Producto = {p.Producto}, Cantidad_Total_Vendida = {p.Cantidad_Total_Vendida} ");
                }
            }
        }
    }
}
