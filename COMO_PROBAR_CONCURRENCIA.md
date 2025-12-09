## 🚀 GUÍA RÁPIDA - CÓMO PROBAR EL CONTROL DE CONCURRENCIAS

### **📁 ARCHIVOS CREADOS PARA TUS PRUEBAS:**

1. **`Tools/TestConcurrencia.ps1`** - Script PowerShell para lanzar múltiples instancias
2. **`Tools/TestConcurrencia.bat`** - Script CMD alternativo 
3. **`ProyectoSauna.Tests/AutomatedConcurrencyTest.cs`** - Pruebas automáticas
4. **`Docs/Manual_Pruebas_Concurrencia.md`** - Manual completo

---

### **🎯 MÉTODO MÁS SIMPLE (RECOMENDADO):**

#### **Paso 1: Ejecutar el script**
```cmd
# MÉTODO SÚPER FÁCIL - Doble clic en el archivo:
C:\Users\pumaq\Music\WPFSauna\PruebasConcurrencia.bat

# O desde terminal (cualquier carpeta):
cd C:\Users\pumaq\Music\WPFSauna
.\PruebasConcurrencia.bat

# O desde la carpeta Tools:
cd C:\Users\pumaq\Music\WPFSauna\Tools  
.\LanzarPruebas.bat
```

#### **Paso 2: Realizar pruebas manuales**
1. ✅ Se abrirán 3 ventanas del programa automáticamente
2. ✅ En TODAS las ventanas: Ir a módulo "Cuentas"
3. ✅ Buscar cliente con DNI: `12345678`
4. ✅ Crear una cuenta nueva
5. ✅ Buscar un producto con POCO stock (2-3 unidades)
6. ✅ **PRUEBA CRÍTICA**: En TODAS las ventanas AL MISMO TIEMPO, intentar agregar el MISMO producto
7. ✅ **RESULTADO ESPERADO**: Solo UNA ventana tendrá éxito, las demás dirán "Stock insuficiente"

---

### **🔬 PRUEBAS AUTOMÁTICAS:**

#### **Ejecutar tests unitarios existentes:**
```cmd
cd C:\Users\pumaq\Music\WPFSauna\ProyectoSauna.Tests
dotnet test
```

#### **Ejecutar pruebas de concurrencia automáticas:**
```cmd
cd C:\Users\pumaq\Music\WPFSauna\ProyectoSauna.Tests
dotnet run AutomatedConcurrencyTest.cs
```

---

### **🎓 PARA PRESENTACIÓN/DEMOSTRACIÓN:**

**Guión sugerido:**
1. 🎬 **"Vamos a simular un problema real: 2 clientes quieren comprar el último producto"**
2. 🛠️ **Ejecutar**: `TestConcurrencia.bat`
3. 📺 **Mostrar**: 3 ventanas del programa abiertas
4. 🎯 **Demostrar**: Concurrencia en acción con producto de stock limitado
5. ✅ **Resultado**: Sistema previene overselling automáticamente

---

### **📊 QUÉ VALIDAR:**

✅ **Éxito del sistema:**
- Solo se vende stock disponible real
- Mensajes claros al usuario sobre conflictos
- Totales de cuenta correctos sin duplicaciones
- Cuentas pagadas no se pueden modificar

❌ **Problemas a detectar:**
- Stock negativo en la base de datos
- Ventas de más productos de los disponibles
- Aplicación se cuelga o crashea
- Errores sin mensajes claros

---

### **💡 CONSEJOS PARA TUS PRUEBAS:**

1. **Usar datos reales**: Los scripts usan los datos maestros ya insertados
2. **Productos de stock bajo**: Busca productos con 1-5 unidades para probar mejor
3. **Timing**: Intenta hacer las operaciones lo más simultáneo posible
4. **Revisar BD**: Después de las pruebas, verifica que los datos sean consistentes

**¡El sistema está listo y probado! 🎉**