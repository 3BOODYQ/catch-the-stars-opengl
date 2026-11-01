# OpenGL learning roadmap

## Goal and approach

Learn how C# sends geometry, textures, and lighting information to the GPU and
build an interactive 3D scene. Progress through small runnable examples, explain
the important lines, and make one independent change after each lesson.

The current stack is C#, .NET 10, OpenTK 4.9.4, and OpenGL 3.3 core. The first
small game, Catch the Stars, is implemented. Rebuild it with the
[step-by-step guide](build-it-yourself.md); the advanced 3D topics below remain
future lessons. Spend extra time on C# foundations if needed.

## Stages

1. **C# foundations:** Variables, functions, loops, arrays, classes, readonly
   record structs, value semantics, `using`, and building with .NET. Practice updating an
   object's position over time in a small console program.
2. **First window:** Use .NET to build the program and OpenTK to create a window,
   handle input, and call OpenGL. Learn the render loop.
   Deliver a colored background, framebuffer resize handling, Escape to close,
   startup error messages, and printed OpenGL version information.
3. **First triangle:** Learn vertices, coordinates, vertex buffers, vertex array
   objects, index buffers, vertex shaders, and fragment shaders. Draw a triangle,
   then a rectangle. Explain how vertex data becomes pixels.
4. **Shader control:** Learn GLSL, shader inputs and outputs, uniforms, shader
   compilation and linking diagnostics, and animation. Change a shape's color
   and position using time and keyboard input.
5. **Textures:** Learn image loading, texture coordinates, filtering, wrapping,
   and transparency. Blend two images on a rectangle. Diagnose a flipped image
   and an incorrect asset path.
6. **3D mathematics:** Introduce vectors and matrices as needed. Use OpenTK.Mathematics for
   translation, rotation, scaling, and perspective. Draw a rotating textured cube
   and enable depth testing so closer surfaces hide farther ones.
7. **Camera:** Combine model, view, and projection matrices with WASD movement,
   mouse look, and elapsed-time-based motion. Explore a scene of cubes. Check
   consistent movement at different frame rates and correct resize proportions.
8. **Lighting:** Learn normals, ambient and diffuse light, specular highlights,
   materials, and directional, point, and spot lights. Build a small lit room.
9. **Rendering techniques:** Introduce face culling, blending, framebuffers, and
   post-processing. Add a grayscale effect, then model loading and shadow mapping
   in separate lessons. Practice diagnosing black screens and missing objects.
10. **Final scene:** Combine camera movement, models, textures, lighting, and one
    visual effect in a small explorable room or outdoor scene. Verify builds,
    startup diagnostics, resizing, resource cleanup, and performance.

## Current coding milestone

Catch the Stars combines a window, shaders, triangles, keyboard input, immutable
game rules, collision, and a bitmap HUD. It demonstrates the first stages without
textures or 3D. Start from the guide's empty-folder setup and reconstruct it one
checkpoint at a time before moving on to textures and a cube.

## Later electives

After the core stages: instancing, normal mapping, HDR, bloom, physically based
rendering, deferred rendering, and GPU profiling. Compute shaders require a
newer OpenGL context than the proposed 3.3 baseline.
