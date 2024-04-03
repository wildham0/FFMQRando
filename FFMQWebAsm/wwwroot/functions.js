//const localforage = require("./localforage.min");
//import localforage from 'localforage.min';

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: "application/octet-stream" });

    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}

async function loadSavedRom() {
    localforage.config();
    value = await localforage.getItem('SavedRom');
    return value;
}

async function saveSavedRom(romToSave) {
    localforage.config();
    localforage.setItem('SavedRom', romToSave).then(function (value) { return true; }).catch(function (err) { return false; });
}

async function loadSavedSprites() {
    localforage.config();
    value = await localforage.getItem('SavedSprites');
    return value;
}

async function saveSavedSprites(spritesToSave) {
    localforage.config();
    localforage.setItem('SavedSprites', spritesToSave).then(function (value) { return true; }).catch(function (err) { return false; });
}

function hideLoadingBox() {
    // Get element with the specified ID name
    var idValue = document.getElementById("loadingbox");
    // Get element with the specified Class name
    idValue.style.setProperty("display", "none");
}

function hideBody() {
    document.getElementsByClassName("content")[0].style.display = "none";
}

function showBody() {
    document.getElementsByClassName("content")[0].style.display = "initial";
}

function randomLoadingIcon() {
    var paths = [
        "mqlogoc.png",
        "mqlogoe.png",
    ]
    document.getElementById('loadingimg').value = "loading/" + paths[Math.floor(Math.random() * paths.length)];
}

function blazorScrollToId(id) {
    const element = document.getElementById(id);
    if (element instanceof HTMLElement) {
        element.scrollIntoView({
            behavior: "auto",
            block: "start",
            inline: "nearest"
        });
    }
}