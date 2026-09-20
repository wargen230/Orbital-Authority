using System;
using Microsoft.Extensions.DependencyInjection;
using OrbitalAuthority.Client;
using OrbitalAuthority.ECS.Core;
using OrbitalAuthority.Gameplay;
using OrbitalAuthority.Physics;
using OrbitalAuthority.Physics.Integrators;

// --- 1. Настраиваем DI ---
var services = new ServiceCollection();

// Ядро
services.AddSingleton<World>();

// Физика — вот эти строки критичны
services.AddSingleton<IIntegrator, VelocityVerletIntegrator>();
services.AddSingleton<IGravityField, NewtonianGravityField>();
services.AddSingleton<PhysicsPipeline>();

// Фабрики
services.AddSingleton<SolarSystemFactory>();

var provider = services.BuildServiceProvider();

// --- 2. Создаём Солнечную систему ОДИН РАЗ ---
var world = provider.GetRequiredService<World>();
var factory = provider.GetRequiredService<SolarSystemFactory>();
factory.Create(world);

Console.WriteLine($"Entities created: {world.EntityCount}");

// --- 3. Передаём provider в игру ---
// ВАЖНО: НЕ создаём второй World!
using var game = new OrbitalAuthorityGame(provider);
game.Run();