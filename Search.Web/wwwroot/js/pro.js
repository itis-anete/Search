var data = ["Kek","Cheburek","ulo","opop"];
var countSearch=data.length;

function seachInfo(countSearch){
    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight,1, 1000);
    camera.position.z=17;
    camera.position.x=-20;
    camera.position.y=5;
    
    var renderer = new THREE.WebGLRenderer({ alpha: true });
    renderer.setSize(window.innerWidth/1.075, window.innerHeight/2);
    
    //scene.background=new THREE.Color("rgb(67, 26, 88)");
    document.body.appendChild(renderer.domElement);
            
    controls=new THREE.OrbitControls(camera,renderer.domElement);
    controls.enableZoom = false;
    controls.enablePan = false;

    var cubeMaterialArray=[];

    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0xEBC1F6}));//зад
    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0xEBC1F6}));//перед
    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0x693776})); //вверх
    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0x693776})); //низ
    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0x4E2458})); //правый бок
    cubeMaterialArray.push(new THREE.MeshBasicMaterial({color:0x4E2458})); //левый бок

    var cubeMaterials=new THREE.MeshFaceMaterial(cubeMaterialArray);

    window.addEventListener('resize',function()
    {
        var width=window.innerWidth/1.09;
        var height=window.innerHeight/2;
        renderer.setSize(width,height);
        camera.aspect=width/height;
        camera.updateProjectionMatrix();
    }
    );
    /*var length = 0.000001, width = 0.5;

    var shape = new THREE.Shape();
    shape.moveTo( 0,0 );
    shape.lineTo( 0, width );
    shape.lineTo( length, width );
    shape.lineTo( length, 0 );
    shape.lineTo( 0, 0 );

    var extrudeSettings = {
        steps: 1,
        depth: 17,
        bevelEnabled: true,
        bevelThickness: 0.5,
        bevelSize: 1,
        bevelSegments: 7
        };

    //create the shape
    var geometry = new THREE.ExtrudeBufferGeometry( shape, extrudeSettings );
        */
       var geometry = new THREE.BoxGeometry( 1, 7, 21 );
    //create a naterial,colour or image texture
    var material = new THREE.MeshNormalMaterial();
    var mesh = new THREE.Mesh( geometry, cubeMaterials ) ;
    //geometry.position.set(0,0,0);
    scene.add(mesh);

   // var anbientLight=new THREE.PointLight(0xFFFFFF, 0.3, 89);
   // anbientLight.position.set( 10, 10, 10 );
    //scene.add(anbientLight);

    //draw Scene
    var render = function ()
    {
        renderer.render(scene, camera);
    };

    var Loop = function () {
        requestAnimationFrame(Loop);

        render();
    };
    Loop();
}