window.easyMarketPosOffline = (() => {
    const dbName = "EasyMarketPosOffline";
    const version = 1;
    const stores = {
        catalogoCache: "catalogoCache",
        ventasCola: "ventasCola"
    };

    function openDb() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, version);
            request.onupgradeneeded = () => {
                const db = request.result;
                if (!db.objectStoreNames.contains(stores.catalogoCache)) {
                    const store = db.createObjectStore(stores.catalogoCache, { keyPath: "productoId" });
                    store.createIndex("codigoBarra", "codigoBarra", { unique: false });
                    store.createIndex("bodegaId", "bodegaId", { unique: false });
                }

                if (!db.objectStoreNames.contains(stores.ventasCola)) {
                    const store = db.createObjectStore(stores.ventasCola, { keyPath: "localQueueId" });
                    store.createIndex("fechaHoraLocal", "fechaHoraLocal", { unique: false });
                    store.createIndex("estadoLocal", "estadoLocal", { unique: false });
                }
            };
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async function withStore(storeName, mode, action) {
        const db = await openDb();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, mode);
            const store = transaction.objectStore(storeName);
            const result = action(store);
            transaction.oncomplete = () => resolve(result);
            transaction.onerror = () => reject(transaction.error);
        });
    }

    function requestToPromise(request) {
        return new Promise((resolve, reject) => {
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    return {
        isOnline: () => navigator.onLine,
        saveCatalogo: async (catalogo) => {
            await withStore(stores.catalogoCache, "readwrite", (store) => {
                store.clear();
                (catalogo?.productos || catalogo || []).forEach((item) => store.put(item));
            });
        },
        getCatalogo: async () => {
            const db = await openDb();
            const transaction = db.transaction(stores.catalogoCache, "readonly");
            return await requestToPromise(transaction.objectStore(stores.catalogoCache).getAll());
        },
        enqueueVenta: async (venta) => {
            await withStore(stores.ventasCola, "readwrite", (store) => store.put(venta));
        },
        getVentasPendientes: async () => {
            const db = await openDb();
            const transaction = db.transaction(stores.ventasCola, "readonly");
            const ventas = await requestToPromise(transaction.objectStore(stores.ventasCola).getAll());
            return ventas.sort((a, b) => String(a.fechaHoraLocal).localeCompare(String(b.fechaHoraLocal)));
        },
        removeVenta: async (localQueueId) => {
            await withStore(stores.ventasCola, "readwrite", (store) => store.delete(localQueueId));
        },
        clearVentas: async () => {
            await withStore(stores.ventasCola, "readwrite", (store) => store.clear());
        },
        registerNetworkHandlers: (dotNetRef) => {
            const onlineHandler = () => dotNetRef.invokeMethodAsync("OnBrowserOnline");
            const offlineHandler = () => dotNetRef.invokeMethodAsync("OnBrowserOffline");
            window.addEventListener("online", onlineHandler);
            window.addEventListener("offline", offlineHandler);
            return {
                dispose: () => {
                    window.removeEventListener("online", onlineHandler);
                    window.removeEventListener("offline", offlineHandler);
                }
            };
        }
    };
})();
