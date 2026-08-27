package com.uisrael.discount_service.Service;
import com.uisrael.discount_service.model.Descuento;
import com.uisrael.discount_service.Repository.DescuentoRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class DescuentoService {

    private final DescuentoRepository repo;

    public DescuentoService(DescuentoRepository repo) {
        this.repo = repo;
    }

    public List<Descuento> listarTodos() {
        return repo.findAll();
    }

    public List<Descuento> listarActivos() {
        return repo.findByEstado("ACTIVO");
    }

    public Optional<Descuento> obtenerActivoPorProducto(Integer productoId) {
        return repo.findFirstByProductoIdAndEstado(productoId, "ACTIVO");
    }
}
