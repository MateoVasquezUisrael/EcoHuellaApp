package com.uisrael.discount_service.controller;


import com.uisrael.discount_service.model.Descuento;
import com.uisrael.discount_service.Service.DescuentoService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/descuentos")
public class DescuentoController {

    private final DescuentoService service;

    public DescuentoController(DescuentoService service) {
        this.service = service;
    }

    // GET /descuentos  -> lista todos
    @GetMapping
    public List<Descuento> listar() {
        return service.listarTodos();
    }

    // GET /descuentos/activos -> lista activos
    @GetMapping("/activos")
    public List<Descuento> listarActivos() {
        return service.listarActivos();
    }

    // GET /descuentos/producto/{id}/activo -> descuento activo por producto
    @GetMapping("/producto/{productoId}/activo")
    public ResponseEntity<Descuento> activoPorProducto(@PathVariable Integer productoId) {
        return service.obtenerActivoPorProducto(productoId)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }
}