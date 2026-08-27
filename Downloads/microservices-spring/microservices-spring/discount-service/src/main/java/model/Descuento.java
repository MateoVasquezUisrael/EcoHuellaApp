package model;

import jakarta.persistence.*;
import java.math.BigDecimal;

@Entity
@Table(name = "Descuentos")

public class Descuento {
	 @Id
	    @GeneratedValue(strategy = GenerationType.IDENTITY)
	    private Integer id;

	    @Column(name = "producto_id", nullable = false)
	    private Integer productoId;

	    @Column(name = "descuento", nullable = false)
	    private BigDecimal descuento;

	    @Column(nullable = false)
	    private String estado;

	    private String observacion;

	    // ===== GETTERS / SETTERS =====
	    public Integer getId() { return id; }
	    public void setId(Integer id) { this.id = id; }

	    public Integer getProductoId() { return productoId; }
	    public void setProductoId(Integer productoId) { this.productoId = productoId; }

	    public BigDecimal getDescuento() { return descuento; }
	    public void setDescuento(BigDecimal descuento) { this.descuento = descuento; }

	    public String getEstado() { return estado; }
	    public void setEstado(String estado) { this.estado = estado; }

	    public String getObservacion() { return observacion; }
	    public void setObservacion(String observacion) { this.observacion = observacion; }
}
