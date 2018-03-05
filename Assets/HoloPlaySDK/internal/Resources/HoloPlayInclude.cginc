float2 texArr(float3 uvz, float tilesX, float tilesY) 
{
    //decide which section to take from based on the z.
    float z = floor(uvz.z);
    float x = fmod(z, tilesX) / tilesX;
    float y = floor(z / tilesX) / tilesY;
    x += uvz.x / tilesX;
    y += uvz.y / tilesY;
    return float2(x, y);
}