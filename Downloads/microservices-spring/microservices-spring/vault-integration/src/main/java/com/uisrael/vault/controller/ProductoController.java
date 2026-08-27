package com.uisrael.vault.controller;


import com.uisrael.vault.models.Producto;
import com.uisrael.vault.repository.ProductoRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/vault")
public class ProductoController {
    @Autowired
    private ProductoRepository repo;

    @GetMapping("/productos")
    public List<Producto> getProductos() {
        return repo.findAll();
    }

    @PostMapping("/productos")
    public Producto createProducto(@RequestBody Producto producto) {
        return repo.save(producto);
    }
}
