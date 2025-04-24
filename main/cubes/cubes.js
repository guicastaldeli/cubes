import * as THREE from 'three';
import * as CANNON from 'https://cdn.jsdelivr.net/npm/cannon-es/dist/cannon-es.js';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';
import * as TWEEN from 'https://cdn.jsdelivr.net/npm/@tweenjs/tween.js@18.6.4/dist/tween.esm.js'

import { camera } from '../main/camera.js';
import { cameraControls } from '../main/camera.js';
import { world } from '../main/main.js';
import { scene } from '../main/main.js';

export function createCubes() {
    const cubes = [];
    const geometries = [];
    const moveSpeed = 0.01;
    const zMin = -40;
    const zMax = 50;
    
    for(let i = 0; i < 500; i++) {
        //Pos
        const rx = Math.floor(Math.random() * (zMax - zMin) + zMin) / 1.7;
        const ry = Math.floor(Math.random() * (zMax - zMin) + zMin) / 1.5;
        const rz = Math.floor(Math.random() * (zMax - zMin) + zMin);

        const pos = {
            x: rx,
            y: ry,
            z: rz
        }
    
        const scale = {
            width: Math.random() * 2 + 0.5,
            height: Math.random() * 2 + 0.5,
            depth: Math.random() * 2 + 0.5
        }

        const geometry = new THREE.BoxGeometry(scale.width, scale.height, scale.depth);
        geometries.push(geometry);

        const randomColor = new THREE.Color();
        randomColor.setHSL(Math.random() * 360, 0.5 + Math.random() * 0.3, 0.3 + Math.random() * 0.2);

        const material = new THREE.MeshStandardMaterial({ color: randomColor, transparent: true, opacity: 1, fog: true });
        const cube = new THREE.Mesh(geometry, material);

        material.depthWrite = true;
        material.depthTest = true;

        cube.receiveShadow = true;
    
        cube.position.x = pos.x;
        cube.position.y = pos.y;
        cube.position.z = pos.z;
    
        //Collision
        const CUBES_GROUP = 1;
        const PLAYER_GROUP = 2;
    
        const playerMaterial = new CANNON.Material('player');
        const cubeMaterial = new CANNON.Material('cube');
    
        const contactMaterial = new CANNON.ContactMaterial(
            playerMaterial, cubeMaterial,
            {
                friction: 0.1,
                restitution: 0.1,
            }
        )

        const cubesColSize = new CANNON.Vec3(scale.width / 2, scale.height / 2, scale.depth / 2);
    
        const cubesBody = new CANNON.Body({
            mass: 0,
            position: new CANNON.Vec3(pos.x, pos.y, pos.z),
            shape: new CANNON.Box(cubesColSize),
            type: CANNON.Body.DYNAMIC
        });

        cubesBody.position.set(pos.x, pos.y, pos.z)
    
        cubesBody.collisionFilterGroup = CUBES_GROUP;
        cubesBody.collisionFilterMask = PLAYER_GROUP;
        
        //Box Helper
            const geometryc = new THREE.BoxGeometry(cubesColSize.x * 2, cubesColSize.y * 2, cubesColSize.z * 2);
            const materialc = new THREE.MeshBasicMaterial({ color: 0x00f00, wireframe: true });
            const cubec = new THREE.Mesh(geometryc, materialc);
        
            cubec.position.set(cubesBody.position.x, cubesBody.position.y, cubesBody.position.z);
        
            const boxHelper = new THREE.BoxHelper(cubec, 0xffff00);
            boxHelper.update();
        
            //scene.add(boxHelper);
        //
    
        world.addContactMaterial(contactMaterial);
        world.addBody(cubesBody);

        const outlineGeometry = new THREE.EdgesGeometry(geometry);
        const outlineMaterial = new THREE.LineBasicMaterial({ color: 'white', side: THREE.BackSide, linewidth: 15 });
        const outline = new THREE.LineSegments(outlineGeometry, outlineMaterial);

        outline.visible = false;

        cube.add(outline);

        cubes.push({ cube, cubesBody, outline });
    }
    
    //Merged Cubes
        const mergedGeometry = mergeGeometries(geometries);
        mergedGeometry.computeBoundingBox();
        mergedGeometry.computeBoundingSphere();
        
        //Materal
            const colors = new Float32Array(mergedGeometry.attributes.position.count * 3);

            for(let i = 0; i < colors.length; i += 3) {
                const randomColor = new THREE.Color();
                randomColor.setHSL(Math.random() * 0.5 + 1, 0.2 + Math.random() * 0.3, 0.3 + Math.random() * 0.2);

                colors[i] = randomColor.r;
                colors[i + 2] = randomColor.g;
                colors[i + 3] = randomColor.b;
            }

            mergedGeometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

            const mergedMaterial = new THREE.MeshStandardMaterial({ vertexColors: true, transparent: true, opacity: 1, fog: true });
        //

        const mergedCubes = new THREE.Mesh(mergedGeometry, mergedMaterial);
        mergedCubes.visible = false;

        scene.add(mergedCubes);
    //

    //Audio
        const listener = new THREE.AudioListener();

        cubes.forEach(({ cube }) => {
            cube.add(listener);
        });

        const audioLoader = new THREE.AudioLoader();
        let cubeSound;

        audioLoader.load('/assets/audio/game/cubes.ogg', (buffer) => {
            cubeSound = new THREE.Audio(listener);
            cubeSound.setBuffer(buffer);
            cubeSound.setLoop(false);
            cubeSound.setVolume(0.5);
        })
    //

    //Raycaster
    const updateRaycaster = () => {
        if(cameraControls.isPointerLocked) {
            const direction = new THREE.Vector3();
            camera.getWorldDirection(direction);

            const raycaster = new THREE.Raycaster(camera.position, direction);

            cubes.forEach(item => {
                if(item.outline) {
                    item.outline.visible = false;
                }

                if(item.currentClickHandler) {
                    window.removeEventListener('click', item.currentClickHandler);
                    item.currentClickHandler = null;
                }
            });

            let closestIntersectedCube = null;
            let closestDistance = Infinity;

            cubes.forEach(({ cube, outline }) => {
                const intersects = raycaster.intersectObjects([cube], true);
    
                if(intersects.length > 0) {
                    const intersection = intersects[0];
                    const distance = intersection.distance;
                    const maxDistance = 8;

                    const bBox = new THREE.Box3().setFromObject(cube);
                    const size = new THREE.Vector3();
                    bBox.getSize(size);
    
                    if (distance <= maxDistance && distance < closestDistance) {
                        closestDistance = distance;
                        closestIntersectedCube = { cube, outline }
                    }
                }
            });

            if(closestIntersectedCube) {
                const { cube, outline } = closestIntersectedCube;
                
                outline.visible = true;

                if(!cube.currentClickHandler) {
                    const originalColor = cube.material.color.clone();
                    const originalOpacity = cube.material.opacity;

                    const cubeClick = () => {
                        const newDirection = new THREE.Vector3();
                        camera.getWorldDirection(newDirection);
                        const newRaycaster = new THREE.Raycaster(camera.position, newDirection);
                        const newIntercets = newRaycaster.intersectObjects([cube], true);

                        const isStillIntersected = newIntercets.length > 0 && 
                        newIntercets[0].distance <= 8 && 
                        newIntercets.length >= Math.floor(Math.cbrt(
                        new THREE.Box3().setFromObject(cube).getSize(new THREE.Vector3()).x *
                        new THREE.Box3().setFromObject(cube).getSize(new THREE.Vector3()).y *
                        new THREE.Box3().setFromObject(cube).getSize(new THREE.Vector3()).z) * 3);

                        //Audio
                        if(isStillIntersected) {
                            if(cubeSound && !cubeSound.isPlaying) {
                                cubeSound.play();
                            }
                        }

                        if(!(isStillIntersected || cube.animating)) return;
                        cube.animating = true;

                        new TWEEN.Tween(cube.material)
                            .to({ color: new THREE.Color(1, 1, 1) }, 175)
                            .easing(TWEEN.Easing.Quadratic.Out)
                            .onComplete(() => {
                                new TWEEN.Tween(cube.material)
                                .to({ color: originalColor, opacity: originalOpacity }, 175)
                                .easing(TWEEN.Easing.Quadratic.In)
                                .onComplete(() => {
                                    cube.animating = false;
                                })
                            .start();
                        })
                        .start();
                    }
                    
                    cube.currentClickHandler = cubeClick;
                    window.addEventListener('click', cubeClick);
                }
            }
        }
    }

    function updateCubesMov() {
        cubes.forEach(item => {
            const { cube, cubesBody } = item;

            cube.position.z += moveSpeed;
            cubesBody.position.z = cube.position.z;

            if(cube.position.z > zMax) {
                cube.position.z = zMin;
                cubesBody.position.z = zMin;
            } else if(cube.position.z < zMin) {
                cube.position.z = zMax;
                cubesBody.position.z = zMax;
            }
        })
    }

    function update() {
        TWEEN.update()
        updateCubesMov();
        updateRaycaster();

        world.step(1 / 60);

        requestAnimationFrame(update);
    }

    update();

    return cubes;
}
