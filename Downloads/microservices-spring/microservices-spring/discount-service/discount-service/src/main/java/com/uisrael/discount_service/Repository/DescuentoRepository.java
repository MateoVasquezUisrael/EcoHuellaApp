package com.uisrael.discount_service.Repository;

import com.uisrael.discount_service.model.Descuento;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;
import java.util.Optional;

public interface DescuentoRepository extends JpaRepository<Descuento, Integer> {

    List<Descuento> findByEstado(String estado);

    Optional<Descuento> findFirstByProductoIdAndEstado(Integer productoId, String estado);
}