# 🌊 STR – Alerta Temprana de Inundaciones (C#)

Sistema de Tiempo Real (STR) desarrollado en **C#** que simula la **detección y emisión de alertas por inundaciones** en zonas vulnerables de la **República Dominicana**.

El sistema procesa datos de sensores en tiempo real y genera alertas automáticas cuando se superan umbrales críticos, cumpliendo **deadlines estrictos**.

---

## 🎯 Objetivo
- Procesar datos de sensores en tiempo real  
- Evaluar riesgos por zona  
- Generar alertas dentro de límites de tiempo  
- Aplicar conceptos de **Ingeniería de Software en Tiempo Real**

---

## ⏱ Tipo de STR
- **Hard Real-Time:** detección y generación de alertas  
- **Soft Real-Time:** registro y análisis de eventos  

Una alerta tardía se considera un fallo del sistema.

---

## 🏗 Arquitectura
1. Sensores simulados  
2. Ingesta de datos  
3. Validación  
4. Evaluación de riesgo  
5. Emisión de alertas (consola)

---

## 📋 Estados de Riesgo
- Normal  
- Vigilancia  
- Alerta  
- Emergencia  

---

## ⏲ Deadlines
| Tarea              | Deadline |
|-------------------|----------|
| Ingesta           | 200 ms   |
| Validación        | 300 ms   |
| Evaluación        | 500 ms   |
| Alerta            | 2 s      |

---

## 💻 Ejecución

### Requisitos
- .NET 6 o superior

### Ejecutar
```bash
dotnet run
```

---

## ⌨ Uso en Consola
Durante la ejecución puedes cambiar el escenario:

```text
[1] Normal
[2] Vigilancia
[3] Alerta
[4] Emergencia
```

Esto permite observar:
- Cumplimiento de deadlines  
- Cambios de estado  
- Generación de alertas  
- Manejo de errores  

---

## 🎥 Video
- **YouTube:** (enlace)
- **Microsoft 365:** (enlace)

---

## 📂 Estructura
```
STR-Alerta-Inundaciones-RD/
├── README.md
└── src/
    └── STRFloodAlert/
        └── Program.cs
```

---

## 👤 Autor
**Miguel Ángel Cruz Fernández**  
Matrícula: **24-0195**

**Asignatura:**  
TI3521-01-2026-2 – Ingeniería de Software en Tiempo Real
