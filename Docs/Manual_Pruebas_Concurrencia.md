# 📋 MANUAL DE PRUEBAS DE CONCURRENCIA - SAUNA KALIXTO

## 🎯 OBJETIVO
Validar que el sistema maneja correctamente múltiples usuarios modificando los mismos datos simultáneamente.

## 🛠️ MÉTODOS DE PRUEBA

### 1️⃣ **PRUEBAS MANUALES (Recomendado para demostración)**

#### **Método A: Script PowerShell**
```powershell
# Ejecutar desde la carpeta Tools/
.\TestConcurrencia.ps1 -NumeroInstancias 3
```

#### **Método B: Ejecutar manualmente**
1. Abrir 3-4 terminales en la carpeta del proyecto
2. En cada terminal ejecutar: `dotnet run`
3. Esperar a que todas las instancias abran

#### **🧪 ESCENARIOS DE PRUEBA:**

**Escenario 1: Stock Concurrente**
1. ✅ Abrir módulo "Cuentas" en todas las instancias
2. ✅ Buscar cliente: DNI `12345678` (usar datos maestros)
3. ✅ Crear cuenta en una instancia
4. ✅ En TODAS las instancias, buscar producto con POCO stock (ej: 2-3 unidades)
5. ✅ Intentar agregar el MISMO producto simultáneamente
6. ✅ **RESULTADO ESPERADO**: Solo una operación exitosa, las demás muestran "Stock insuficiente"

**Escenario 2: Modificación de Cuenta**
1. ✅ Tener la MISMA cuenta abierta en varias instancias
2. ✅ Agregar productos diferentes en cada instancia AL MISMO TIEMPO
3. ✅ **RESULTADO ESPERADO**: Los totales se actualizan correctamente sin duplicar

**Escenario 3: Estado de Cuenta**
1. ✅ Procesar pago de una cuenta en una instancia
2. ✅ Intentar modificar la misma cuenta en otra instancia
3. ✅ **RESULTADO ESPERADO**: Mensaje "No se puede modificar cuenta Pagada"

---

### 2️⃣ **PRUEBAS AUTOMATIZADAS**

#### **Ejecutar tests unitarios:**
```powershell
# En la carpeta del proyecto de pruebas
dotnet test --logger console
```

#### **Ejecutar pruebas de concurrencia automáticas:**
```powershell
# Compilar el programa de pruebas
dotnet run --project ProyectoSauna.Tests
```

---

## 📊 **MÉTRICAS A VALIDAR**

### ✅ **Indicadores de Éxito:**
- **Stock**: Solo se descuenta la cantidad real disponible
- **Totales**: Los cálculos son consistentes sin duplicaciones
- **Estados**: Las cuentas pagadas no se pueden modificar
- **Errores**: Mensajes claros de concurrencia al usuario
- **Performance**: No bloqueos indefinidos

### ❌ **Indicadores de Problema:**
- Stock negativo en la base de datos
- Totales duplicados o incorrectos
- Aplicación se congela o crashea
- Operaciones que no deberían permitirse se ejecutan
- Mensajes de error confusos

---

## 🔍 **DATOS DE PRUEBA SUGERIDOS**

### **Clientes de Prueba:**
```sql
-- DNI: 12345678 (Cliente: Juan Pérez)
-- DNI: 87654321 (Cliente: María García)
-- DNI: 11223344 (Cliente: Carlos López)
```

### **Productos con Stock Limitado:**
```sql
-- Buscar productos con stock <= 5 unidades
SELECT nombre, stockActual FROM Producto 
WHERE stockActual BETWEEN 1 AND 5 AND activo = 1;
```

---

## 🚀 **INSTRUCCIONES DE EJECUCIÓN**

### **Paso 1: Preparar el entorno**
```powershell
# Asegurarse de que la BD tiene datos de prueba
cd C:\Users\pumaq\Music\WPFSauna\Bd
# Ejecutar DATOS_MAESTROS_DE_PRUEBA.sql si es necesario
```

### **Paso 2: Lanzar instancias**
```powershell
# Opción A: Usar script automático
cd C:\Users\pumaq\Music\WPFSauna\Tools
.\TestConcurrencia.ps1

# Opción B: Manual
# Terminal 1: dotnet run
# Terminal 2: dotnet run  
# Terminal 3: dotnet run
```

### **Paso 3: Ejecutar escenarios**
Seguir los escenarios descritos arriba en el orden sugerido.

### **Paso 4: Verificar resultados**
- ✅ Revisar logs en la consola
- ✅ Verificar datos en la base de datos
- ✅ Confirmar que no hay stock negativo
- ✅ Validar que los totales son correctos

---

## 📋 **CHECKLIST DE VALIDACIÓN**

```
□ Script PowerShell ejecuta múltiples instancias correctamente
□ Las instancias se conectan a la misma base de datos
□ Operaciones concurrentes en stock muestran validaciones
□ Totales de cuenta se calculan sin duplicaciones  
□ Estados de cuenta se respetan (Pendiente vs Pagada)
□ Mensajes de error son claros para el usuario
□ No hay excepciones no controladas
□ Performance es aceptable con múltiples usuarios
□ Logs de auditoría registran todas las operaciones
□ Sistema se recupera correctamente de conflictos
```

---

## 🎓 **PARA DEMOSTRACIÓN ACADÉMICA**

**Presentación sugerida:**
1. 🎬 Explicar el concepto de concurrencia en bases de datos
2. 🎯 Mostrar el problema: "¿Qué pasa si 2 usuarios compran el último producto?"
3. 🛡️ Demostrar la solución: Control de concurrencia optimista
4. 🧪 Ejecutar pruebas en vivo con múltiples instancias
5. 📊 Mostrar resultados y validaciones exitosas

**Puntos técnicos a destacar:**
- Entity Framework Concurrency Tokens
- DbUpdateConcurrencyException handling
- Validaciones de negocio thread-safe
- Mensajes de usuario amigables
- Auditoría de operaciones