// SunLight.fx
// Полноэкранная трассировка лучей: один draw call на весь кадр вместо одного на тело.
// Шейдер сам перебирает все тела сцены (переданы как массивы параметров) для каждого пикселя,
// поэтому свет и жёсткие тени от Солнца считаются аналитически (ray-sphere intersection),
// без отдельного прохода shadow map.

// Проект собирается только под платформу DesktopVK (Vulkan) — её MGFX-профиль
// требует шейдерную модель 6.0, DX9-уровневые строки (vs_4_0_level_9_1) тут недопустимы.
#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define MAX_SPHERES 8

// --- Камера (все позиции ниже — camera-relative: камера всегда в (0,0,0)) ---
float3 CameraRight;
float3 CameraUp;
float3 CameraForward;
float TanHalfFovY;
float AspectRatio;

// --- Сцена ---
float3 LightPosition;      // позиция Солнца относительно камеры
float3 BackgroundColor;

int SphereCount;
float3 SpherePositions[MAX_SPHERES];   // относительно камеры
float SphereRadii[MAX_SPHERES];
float3 SphereColors[MAX_SPHERES];
float SphereIsStar[MAX_SPHERES];       // 1 = звезда (самосветящаяся, без теней/освещения)

struct VSInput
{
    float4 Position : POSITION0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 ClipXY   : TEXCOORD0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    // Квад уже в clip space (-1..1), матрицы View/Projection тут не нужны —
    // вся 3D-работа происходит в пиксельном шейдере, через сам луч.
    output.Position = float4(input.Position.xy, 0, 1);
    output.ClipXY = input.Position.xy;
    return output;
}

// Аналитическое пересечение луча со сферой. Возвращает ближайшее t > 0, если есть.
bool IntersectSphere(float3 ro, float3 rd, float3 center, float radius, out float t)
{
    float3 oc = ro - center;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - radius * radius;
    float h = b * b - c; // rd нормализован, поэтому a = 1

    if (h < 0)
    {
        t = -1;
        return false;
    }

    h = sqrt(h);
    float tNear = -b - h;
    float tFar = -b + h;
    t = tNear > 0.0001 ? tNear : tFar;

    return t > 0.0001;
}

float4 MainPS(VSOutput input) : SV_TARGET0
{
    float3 rayOrigin = float3(0, 0, 0);
    float3 rayDir = normalize(
        CameraForward +
        input.ClipXY.x * TanHalfFovY * AspectRatio * CameraRight +
        input.ClipXY.y * TanHalfFovY * CameraUp);

    // --- Ищем ближайшее пересечение среди всех тел сцены ---
    float closestT = 1e30;
    int hitIndex = -1;

    for (int i = 0; i < SphereCount; i++)
    {
        float t;
        if (IntersectSphere(rayOrigin, rayDir, SpherePositions[i], SphereRadii[i], t) && t < closestT)
        {
            closestT = t;
            hitIndex = i;
        }
    }

    if (hitIndex < 0)
    {
        return float4(BackgroundColor, 1);
    }

    float3 hitPoint = rayOrigin + rayDir * closestT;
    float3 normal = normalize(hitPoint - SpherePositions[hitIndex]);
    float3 baseColor = SphereColors[hitIndex];

    // Звезда светит сама — без освещения и теней (иначе половина диска была бы "в тени").
    if (SphereIsStar[hitIndex] > 0.5)
    {
        return float4(baseColor, 1);
    }

    float3 toLight = LightPosition - hitPoint;
    float distToLight = length(toLight);
    float3 lightDir = toLight / distToLight;

    float diffuse = max(dot(normal, lightDir), 0);

    // --- Тень: второй луч к Солнцу, проверяем все ОСТАЛЬНЫЕ тела на пути ---
    // Эпсилон смещения — доля радиуса самого тела (не расстояния до света!),
    // иначе на астрономических масштабах (~1e11 м) offset либо ничтожен, либо огромен.
    float3 shadowOrigin = hitPoint + normal * SphereRadii[hitIndex] * 0.001;

    for (int j = 0; j < SphereCount; j++)
    {
        if (j == hitIndex) continue;
        // Луч нацелен ровно в центр Солнца, поэтому без этой проверки он всегда
        // "врезается" в поверхность самого Солнца чуть раньше t = distToLight —
        // источник света не может затенять сам себя.
        if (SphereIsStar[j] > 0.5) continue;

        float tShadow;
        if (IntersectSphere(shadowOrigin, lightDir, SpherePositions[j], SphereRadii[j], tShadow) && tShadow < distToLight)
        {
            diffuse = 0;
            break;
        }
    }

    float ambient = 0.03; // чтобы ночная сторона не была абсолютно чёрной
    float3 litColor = baseColor * (ambient + diffuse * (1 - ambient));

    return float4(litColor, 1);
}

technique SunLight
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
