package com.example.discount_service.Repository;

import model.Descuento;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;

public interface DescuentoRepository extends JpaRepository<Descuento, Integer> {

    Optional<Descuento> findFirstByProductoIdAndEstado(Integer productoId, String estado);

}