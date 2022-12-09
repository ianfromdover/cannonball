Shader "Unlit/ARCoreFrame"
{
    Properties
    {
        _textureDepthSuppressionMask ("Depth Suppresion Mask", 2D) = "black" {}
        _textureFusedDepth ("Fused Depth", 2D) = "black" {}
        _textureDepth ("Depth", 2D) = "black" {}
        _texture ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest Always
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }
            
            GLSLPROGRAM

            #pragma multi_compile_local __ DEPTH_ZWRITE
            #pragma multi_compile_local __ DEPTH_SUPPRESSION
            #pragma multi_compile_local __ DEPTH_STABILIZATION
            #pragma multi_compile_local __ DEPTH_DEBUG

            #pragma only_renderers gles3

            #include "UnityCG.glslinc"

            #ifdef SHADER_API_GLES3
            #extension GL_OES_EGL_image_external_essl3 : require
            #endif
            
#ifdef VERTEX

            // Transform used to sample the color planes
            uniform mat4 _displayTransform;

            // Transform used to sample the context awareness textures
            uniform mat4 _depthTransform;
            uniform mat4 _semanticsTransform;

            // Transformed UVs
            varying vec2 _colorUV;
            varying vec3 _depthUV;
            varying vec3 _semanticsUV;
            varying vec2 _vertexUV;

            void main()
            {
                #ifdef SHADER_API_GLES3
                
                // Transform UVs for the color texture
                vec4 texCoord = vec4(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y, 0.0f, 1.0f);
                _colorUV = (_displayTransform * texCoord).xy;

                #ifdef DEPTH_ZWRITE

                // Transform UVs for the context awareness textures
                vec4 uv = vec4(gl_MultiTexCoord0.x, gl_MultiTexCoord0.y, 1.0f, 1.0f);
                _depthUV = (_depthTransform * uv).xyz;

                #ifdef DEPTH_SUPPRESSION
                _semanticsUV = (_semanticsTransform * uv).xyz;
                #endif

                #ifdef DEPTH_STABILIZATION
                _vertexUV = uv.xy;
                #endif

                #endif

                // Transform vertex position
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                
                #endif
            }
#endif

#ifdef FRAGMENT

            // Transformed texture coordinates
            varying vec2 _colorUV;
            varying vec3 _depthUV;
            varying vec3 _semanticsUV;
            varying vec2 _vertexUV;

            // Depth range used for scaling
            uniform float _depthScaleMin;
            uniform float _depthScaleMax;

            uniform sampler2D _textureDepth;
            uniform sampler2D _textureFusedDepth;
            uniform sampler2D _textureDepthSuppressionMask;
            
            uniform samplerExternalOES _texture;
            // uniform sampler2D _texture;

#if defined(SHADER_API_GLES3)
#if defined(DEPTH_ZWRITE)

            uniform vec4 _ZBufferParams;

            // Z buffer to linear 0..1 depth
            float Linear01Depth( float z )
            {
                return 1.0f / (_ZBufferParams.x * z + _ZBufferParams.y);
            }

            // Z buffer to linear depth
            float LinearEyeDepth( float z )
            {
                return 1.0f / (_ZBufferParams.z * z + _ZBufferParams.w);
            }
            
            // Inverse of LinearEyeDepth
            float EyeDepthToNonLinear(float eyeDepth)
            {   
                return (1.0f - (eyeDepth * _ZBufferParams.w)) / (eyeDepth * _ZBufferParams.z);
            }         
#endif
#if !defined(UNITY_COLORSPACE_GAMMA)
            vec3 GammaToLinearSpace (vec3 sRGB)
            {
                // This is from Unity, originally from
                // http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return sRGB * (sRGB * (sRGB * 0.305306011F + 0.682171111F) + 0.012522878F);
            }
#endif
#endif            
            void main()
            {      
#ifdef SHADER_API_GLES3

                // Sample color
                vec4 color = texture(_texture, _colorUV);

#ifndef UNITY_COLORSPACE_GAMMA
                color.xyz = GammaToLinearSpace(color.xyz);
#endif

                // Reset depth
                float depth = 1.0f;

    #ifdef DEPTH_ZWRITE
    #ifdef DEPTH_SUPPRESSION
                    // If depth is not suppressed at this pixel
                    vec2 semanticsUV = vec2(_semanticsUV.x / _semanticsUV.z, _semanticsUV.y / _semanticsUV.z);
                    if (texture(_textureDepthSuppressionMask, semanticsUV).x == 0.0f)
    #endif // DEPTH_SUPPRESSION
                    {
                        // Sample depth
                        vec2 depthUV = vec2(_depthUV.x / _depthUV.z, _depthUV.y / _depthUV.z);
                        float rawDepth = texture(_textureDepth, depthUV).x;

                        // Scale depth in case it is normalized
                        // Note: If depth is not normalized, min and max should
                        // be 0 and 1 respectively to leave the value intact
                        float eyeDepth = rawDepth * (_depthScaleMax - _depthScaleMin) + _depthScaleMin;

    #ifdef DEPTH_STABILIZATION
                        
                        // Calculate non-linear frame depth
                        float frameDepth = EyeDepthToNonLinear(eyeDepth);
                        
                        // Sample non-linear fused depth
                        float fusedDepth = texture(_textureFusedDepth, _vertexUV).x;
                        
                        // Linearize and compare
                        float frameLinear = Linear01Depth(frameDepth);
                        float fusedLinear = Linear01Depth(fusedDepth);
                        
                        bool useFrameDepth = fusedLinear == 0.0f || (abs(fusedLinear - frameLinear) / fusedLinear) >= 0.4f;
                        
                        // Write z-buffer
                        depth = useFrameDepth ? frameDepth : fusedDepth;
    #else
                        // Convert to nonlinear and write to the zbuffer
                        depth = EyeDepthToNonLinear(eyeDepth);
    #endif // DEPTH_STABILIZATION

    #ifdef DEPTH_DEBUG
                        // Write disparity to the color channels for debug purposes
                        float MAX_VIEW_DISP = 4.0f;
                        float scaledDisparity = 1.0f / LinearEyeDepth(depth);
                        float normDisparity = scaledDisparity/MAX_VIEW_DISP;
                        color = vec4(normDisparity, normDisparity, normDisparity, 1.0f);

    #ifdef DEPTH_STABILIZATION
                    if (useFrameDepth)
                        color = vec4(normDisparity, normDisparity * 0.5f, normDisparity, 1.0f);
    #endif

    #endif // DEPTH_DEBUG
                    }     
    #endif // DEPTH_ZWRITE
                gl_FragColor = color;
                gl_FragDepth = depth;
#endif // SHADER_API_GLES3
            }

#endif

            ENDGLSL
        }
    }
    
    Fallback "Unlit/ARCoreFrameLegacy"
}
