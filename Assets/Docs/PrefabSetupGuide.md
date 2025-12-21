# Guía Completa: Configuración de Prefabs de Enemigos y Hazards

Esta guía explica **paso a paso** cómo crear los prefabs de todos los enemigos y hazards implementados en el sistema refactorizado.

---

## 📋 Prerrequisitos

Antes de empezar, asegúrate de tener:

1. **Tags definidos en Unity** (Edit → Project Settings → Tags and Layers):
   - `Player` - Para el jugador
   - `Enemy` - Para todos los enemigos (opcional pero recomendado)

2. **Layers configurados**:
   - `Player` - Layer para el jugador
   - `Enemy` - Layer para enemigos

3. **Carpeta de Prefabs**:
   - Crea `Assets/Prefabs/Enemies/`
   - Crea `Assets/Prefabs/Hazards/`
   - Crea `Assets/Prefabs/Projectiles/`

---

## 🦀 CrabEnemy (Cangrejo)

### Estructura del Prefab

```
CrabEnemy (GameObject raíz)
├── Model (tu modelo 3D del cangrejo)
├── PointA (Empty - punto de patrulla)
└── PointB (Empty - punto de patrulla)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `CrabEnemy`
   - Position: (0, 0, 0)

2. **Agregar el modelo visual**:
   - Arrastrar tu modelo 3D del cangrejo como **hijo** de `CrabEnemy`
   - Si no tienes modelo, usar `3D Object → Capsule` temporalmente

3. **Agregar componentes al raíz**:

   | Componente | Configuración |
   |------------|---------------|
   | **CrabEnemy** (script) | Se agregan automáticamente: `EnemyStateMachine`, `PlayerDetector` |
   | **CapsuleCollider** | Center: ajustar al modelo, Radius/Height según tamaño |
   | **Rigidbody** | Use Gravity: ✅, Freeze Rotation: ✅ X, ✅ Z |

4. **Crear puntos de patrulla**:
   - Crear `GameObject → Create Empty` como hijo → Nombrar: `PointA`
   - Crear otro `GameObject → Create Empty` como hijo → Nombrar: `PointB`
   - Posicionar `PointA` a (-3, 0, 0)
   - Posicionar `PointB` a (3, 0, 0)
   - **IMPORTANTE**: Estos puntos se "sueltan" automáticamente al iniciar el juego

5. **Configurar el script CrabEnemy**:
   - **Point A**: Arrastrar `PointA`
   - **Point B**: Arrastrar `PointB`
   - **Detection Radius**: 8 (radio de detección del jugador)
   - **Chase Radius**: 12 (radio para perseguir)
   - **Attack Range**: 2 (distancia de ataque)
   - **Attack Damage**: 1
   - **Move Speed**: 3
   - **Chase Speed**: 5

6. **Configurar PlayerDetector**:
   - **Player Layer**: Seleccionar layer "Player"
   - **Require Line Of Sight**: ❌ (opcional)

7. **Crear el Prefab**:
   - Arrastrar `CrabEnemy` desde la jerarquía a `Assets/Prefabs/Enemies/`

### Animator (Opcional)

Si tienes animaciones:
1. Crear `Animator Controller` en `Assets/Animations/Enemies/CrabController`
2. Agregar parámetros:
   - `IsMoving` (Bool)
   - `IsChasing` (Bool)
   - `IsAttacking` (Bool)
   - `Attack` (Trigger)
   - `Snap` (Trigger)
3. Asignar el controller al componente Animator

---

## 🐡 ExplodingFishEnemy (Pez Explosivo)

### Estructura del Prefab

```
ExplodingFishEnemy (GameObject raíz)
└── Model (modelo 3D del pez)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `ExplodingFishEnemy`

2. **Agregar el modelo visual**:
   - Modelo 3D del pez como hijo
   - **IMPORTANTE**: El modelo necesita un `Renderer` visible para el efecto de flash rojo

3. **Agregar componentes al raíz**:

   | Componente | Configuración |
   |------------|---------------|
   | **ExplodingFishEnemy** (script) | Auto-agrega: `EnemyStateMachine`, `PlayerDetector` |
   | **SphereCollider** | Radius: 0.5 (ajustar al modelo) |
   | **Rigidbody** | Use Gravity: ❌, Drag: 2, Freeze Rotation: ✅ X, ✅ Z |

4. **Configurar el script**:
   - **Explosion Trigger Radius**: 2 (distancia para activar explosión)
   - **Explosion Damage Radius**: 5 (radio de daño)
   - **Explosion Delay**: 1.5 segundos
   - **Explosion Damage Multiplier**: 5x
   - **Warning Color**: Rojo (255, 0, 0)
   - **Flash Speed**: 10
   - **Explosion Effect Prefab**: (Arrastrar partícula de explosión si tienes)

5. **Configurar PlayerDetector**:
   - **Player Layer**: Seleccionar layer "Player"

6. **Crear el Prefab**:
   - Arrastrar a `Assets/Prefabs/Enemies/`

### Prefab de Explosión (Opcional)

1. Crear partícula: `Effects → Particle System`
2. Configurar para explosión corta (0.5s)
3. Guardar como prefab: `Assets/Prefabs/Effects/Explosion`
4. Asignar al campo `Explosion Effect Prefab`

---

## 🐡 SpikyFishEnemy (Pez Espinoso)

### Estructura del Prefab

```
SpikyFishEnemy (GameObject raíz)
├── Model (modelo 3D)
└── Waypoint1, Waypoint2, etc. (opcional)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `SpikyFishEnemy`

2. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **SpikyFishEnemy** (script) | Auto-agrega componentes necesarios |
   | **SphereCollider** | Radius según modelo |
   | **Rigidbody** | Use Gravity: ❌, Drag: 1 |

3. **Configurar el script**:
   - **Spine Prefab**: ⚠️ **REQUERIDO** - Ver sección "SpineProjectile"
   - **Spines Per Burst**: 3
   - **Spine Shoot Interval**: 0.3s
   - **Spine Spread Angle**: 30°
   - **Puff Duration**: 2s
   - **Puff Cooldown**: 4s
   - **Puff Trigger Range**: 6
   - **Puff Scale Multiplier**: 1.5

4. **Crear waypoints (opcional)**:
   - Crear Empty GameObjects como hijos
   - Arrastrar a la lista `Patrol Waypoints`

5. **Crear el Prefab**

---

## 🔷 SpineProjectile (Proyectil de Espina)

### ⚠️ CREAR PRIMERO - Necesario para SpikyFishEnemy

### Estructura del Prefab

```
SpineProjectile (GameObject raíz)
└── Model (espina visual)
```

### Paso a Paso

1. **Crear el GameObject**:
   - `3D Object → Capsule` o tu modelo de espina
   - Nombrar: `SpineProjectile`
   - Scale: pequeño (0.1, 0.3, 0.1)

2. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **SpineProjectile** (script) | |
   | **Rigidbody** | Use Gravity: ❌, Is Kinematic: ❌ |
   | **CapsuleCollider** | Is Trigger: ✅ |

3. **Configurar el script**:
   - **Speed**: 15
   - **Damage**: 1
   - **Knockback**: 5
   - **Lifetime**: 5s
   - **Hit Tags**: `Player`

4. **Crear el Prefab**:
   - Guardar en `Assets/Prefabs/Projectiles/SpineProjectile`

5. **Asignar al SpikyFishEnemy**:
   - Abrir prefab SpikyFishEnemy
   - Arrastrar SpineProjectile al campo `Spine Prefab`

---

## 🎐 JellyfishEnemy (Medusa)

### Estructura del Prefab

```
JellyfishEnemy (GameObject raíz)
├── Model (modelo de medusa)
└── ShockParticles (opcional - partículas eléctricas)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `JellyfishEnemy`

2. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **JellyfishEnemy** (script) | |
   | **SphereCollider** | Radius según modelo |
   | **Rigidbody** | Use Gravity: ❌, Drag: 1, Is Kinematic: ❌ |

3. **Configurar el script**:
   - **Shock Radius**: 4 (radio de daño eléctrico)
   - **Shock Damage**: 0.5 (daño por pulso)
   - **Shock Interval**: 0.5s
   - **Drift Speed**: 1 (velocidad de movimiento flotante)
   - **Drift Change Interval**: 3s
   - **Drift Radius**: 5 (no se alejará más de esto del spawn)
   - **Shock Color**: Azul claro (0.5, 0.5, 1)

4. **Partículas eléctricas (opcional)**:
   - Crear `Effects → Particle System` como hijo
   - Configurar partículas de electricidad/chispas
   - Arrastrar al campo `Shock Particles`

5. **Crear el Prefab**

---

## 🐟 AnglerFishEnemy (Pez Rape)

### Estructura del Prefab

```
AnglerFishEnemy (GameObject raíz)
├── Model (modelo del pez)
└── Lure (Empty con Light)
    └── Point Light
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `AnglerFishEnemy`

2. **Crear el señuelo (Lure)**:
   - Crear `GameObject → Create Empty` como hijo → Nombrar: `Lure`
   - Posicionar donde estaría el señuelo en el pez rape
   - Agregar hijo: `Light → Point Light`
   - Configurar luz: Range: 5, Intensity: 2, Color: Amarillo brillante

3. **Agregar componentes al raíz**:

   | Componente | Configuración |
   |------------|---------------|
   | **AnglerFishEnemy** (script) | |
   | **SphereCollider** | Radius según modelo |
   | **Rigidbody** | Use Gravity: ❌, Is Kinematic: ✅ (emboscador estático) |

4. **Configurar el script**:
   - **Lure Transform**: Arrastrar el objeto `Lure`
   - **Lure Light**: Arrastrar el `Point Light` dentro de Lure
   - **Ambush Range**: 5 (distancia para emboscar)
   - **Lunge Duration**: 0.3s (velocidad del ataque)
   - **Ambush Damage Multiplier**: 3x
   - **Ambush Cooldown**: 4s
   - **Lure Intensity**: 2
   - **Lure Pulse Speed**: 2
   - **Lure Swing Amplitude**: 15°

5. **Crear el Prefab**

---

## 🐙 SupportManta (Manta Raya Plataforma)

### Estructura del Prefab

```
SupportManta (GameObject raíz)
└── Model (modelo de manta raya)
```

### ⚠️ El Spline NO va en el prefab - se configura por escena

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `SupportManta`

2. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **SupportManta** (script) | |
   | **BoxCollider** | Ajustar para que el jugador se pare encima |
   | **Rigidbody** | Is Kinematic: ✅ (IMPORTANTE para plataformas móviles) |

3. **Configurar el script**:
   - **Path**: ⚠️ **NO tocar aquí** - Se asigna en la escena
   - **Speed**: 5
   - **Loop**: ✅
   - **Ping Pong**: ❌
   - **Bob Amplitude**: 0.2
   - **Bob Speed**: 2

4. **Crear el Prefab**

### En la Escena

1. Crear un Spline:
   - `GameObject → Spline → Draw Splines Tool`
   - Dibujar el camino que seguirá la manta
   - Ajustar puntos según necesites

2. Instanciar la manta:
   - Arrastrar prefab `SupportManta` a la escena
   - Arrastrar el `Spline Container` al campo `Path` del script

---

## 🗑️ StickyTrashHazard (Basura Pegajosa)

### Estructura del Prefab

```
StickyTrashHazard (GameObject raíz)
└── Model (bolsa de basura, residuos, etc.)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `StickyTrashHazard`

2. **Agregar modelo visual** como hijo

3. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **StickyTrashHazard** (script) | |
   | **BoxCollider** o **MeshCollider** | ⚠️ **Is Trigger: ✅** IMPORTANTE |

4. **Configurar el script**:
   - **Slow Factor**: 0.5 (50% de reducción de velocidad)
   - **Slow Transition Speed**: 3 (qué tan rápido se aplica)
   - **Impair Vision**: ✅ (oscurece la pantalla)
   - **Overlay Color**: Marrón oscuro transparente (0.1, 0.08, 0.05, 0.6)

5. **Crear el Prefab**

---

## 💨 BubbleStream (Corriente de Burbujas)

### Estructura del Prefab

```
BubbleStream (GameObject raíz)
└── BubbleParticles (Particle System opcional)
```

### Paso a Paso

1. **Crear el GameObject raíz**:
   - `GameObject → Create Empty` → Nombrar: `BubbleStream`

2. **Agregar componentes**:

   | Componente | Configuración |
   |------------|---------------|
   | **BubbleStream** (script) | |
   | **BoxCollider** | Forma de la corriente, **Is Trigger: ✅** |

3. **Configurar el script**:
   - **Upward Force**: 15
   - **Force Ramp Speed**: 3
   - **Max Velocity Boost**: 10

4. **Partículas de burbujas (opcional)**:
   - Crear `Effects → Particle System` como hijo
   - Configurar burbujas subiendo
   - Arrastrar al campo `Bubble Particles`

5. **Crear el Prefab**

6. **En la escena**: Rotar y escalar el BoxCollider para definir la zona de la corriente

---

## ✅ Lista de Verificación Final

### Tags Necesarios
- [ ] `Player` definido
- [ ] `Enemy` definido (opcional pero recomendado)

### Prefabs Creados
- [ ] `CrabEnemy`
- [ ] `ExplodingFishEnemy`
- [ ] `SpikyFishEnemy`
- [ ] `SpineProjectile` ← Requisito de SpikyFish
- [ ] `JellyfishEnemy`
- [ ] `AnglerFishEnemy`
- [ ] `SupportManta`
- [ ] `StickyTrashHazard`
- [ ] `BubbleStream`

### Configuración del Jugador
- [ ] PlayerController tiene Tag: `Player`
- [ ] PlayerController está en Layer: `Player`

### PlayerDetector en Cada Enemigo
- [ ] Player Layer configurado correctamente

---

## 🔧 Debugging

### Si los enemigos no detectan al jugador:
1. Verificar que el jugador tiene el **Tag "Player"**
2. Verificar que el **PlayerDetector** tiene configurado el **Player Layer** correcto
3. Habilitar `Debug States` en el enemigo para ver logs en consola

### Si las animaciones no funcionan:
1. Verificar que el Animator tiene un **AnimatorController asignado**
2. Verificar que el controller tiene los parámetros correctos:
   - `IsMoving`, `IsChasing`, `IsAttacking` (Bools)
   - `Attack`, `Alert`, `Die` (Triggers)

### Si el prefab no daña al jugador:
1. Verificar que el Player implementa **IDamageable**
2. Verificar que los colliders están configurados correctamente
3. Verificar que **Attack Damage** > 0 en el script del enemigo
