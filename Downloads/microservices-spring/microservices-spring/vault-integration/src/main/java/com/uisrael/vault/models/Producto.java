package com.uisrael.vault.models;

import org.springframework.context.annotation.Primary;
import org.springframework.data.annotation.Id;

import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Table;

@Entity
@Table(name = "Producto")
public class Producto {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    private String nombre;
    private Double precio;
    private Boolean iva;
    private Long idcategoria;

    // Getters y setters
    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }
    public String getNombre() { return nombre; }
    public void setNombre(String nombre) { this.nombre = nombre; }
    public Double getPrecio() { return precio; }
    public void setPrecio(Double precio) { this.precio = precio; }
    public Boolean getIva() { return iva; }
    public void setIva(Boolean iva) { this.iva = iva; }
    public Long getIdcategoria() { return idcategoria; }
    public void setIdcategoria(Long idcategoria) { this.idcategoria = idcategoria; }
}