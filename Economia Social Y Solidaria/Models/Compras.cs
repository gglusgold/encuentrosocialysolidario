
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Economia_Social_Y_Solidaria.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Compras
{

    public Compras()
    {

        this.Comentarios = new HashSet<Comentarios>();

        this.ComentariosProducto = new HashSet<ComentariosProducto>();

        this.CompraProducto = new HashSet<CompraProducto>();

    }


    public int idCompra { get; set; }

    public System.DateTime fecha { get; set; }

    public int vecinoId { get; set; }

    public int tandaId { get; set; }

    public int localId { get; set; }

    public int estadoId { get; set; }

    public Nullable<int> hash { get; set; }



    public virtual ICollection<Comentarios> Comentarios { get; set; }

    public virtual ICollection<ComentariosProducto> ComentariosProducto { get; set; }

    public virtual ICollection<CompraProducto> CompraProducto { get; set; }

    public virtual EstadosCompra EstadosCompra { get; set; }

    public virtual Locales Locales { get; set; }

    public virtual Tandas Tandas { get; set; }

    public virtual Vecinos Vecinos { get; set; }

}

}