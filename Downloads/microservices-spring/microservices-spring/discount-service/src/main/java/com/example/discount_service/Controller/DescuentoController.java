package com.example.discount_service.Controller;
import model.Descuento;
import com.example.discount_service.Repository.DescuentoRepository;

import java.util.List;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/descuentos")
public class DescuentoController {


    private final DescuentoRepository repo;

    public DescuentoController(DescuentoRepository repo) {
        this.repo = repo;
    }

    @GetMapping
    public List<Descuento> listar() {
        return repo.findAll();
    }

    @GetMapping("/producto/{productoId}/activo")
    public ResponseEntity<Descuento> activo(@PathVariable Integer productoId) {
        return repo.findFirstByProductoIdAndEstado(productoId, "ACTIVO")
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }
}
