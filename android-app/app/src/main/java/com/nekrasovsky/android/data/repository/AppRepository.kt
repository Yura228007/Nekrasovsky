package com.nekrasovsky.android.data.repository

import com.nekrasovsky.android.data.api.ApiService
import com.nekrasovsky.android.data.model.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AppRepository @Inject constructor(
    private val apiService: ApiService
) {
    // Users
    suspend fun getAllUsers() = apiService.getAllUsers()
    suspend fun getUserById(id: Int) = apiService.getUserById(id)
    suspend fun searchUsers(name: String?, surname: String?) = apiService.searchUsers(name, surname)
    suspend fun addUser(user: User) = apiService.addUser(user)
    suspend fun editUser(id: Int, user: User) = apiService.editUser(id, user)
    suspend fun deleteUser(id: Int) = apiService.deleteUser(id)
    
    // Products
    suspend fun getAllProducts() = apiService.getAllProducts()
    suspend fun getProductById(id: Int) = apiService.getProductById(id)
    suspend fun searchProducts(name: String?, code: String?) = apiService.searchProducts(name, code)
    suspend fun addProduct(product: Product) = apiService.addProduct(product)
    suspend fun editProduct(id: Int, product: Product) = apiService.editProduct(id, product)
    suspend fun deleteProduct(id: Int) = apiService.deleteProduct(id)
    
    // Materials
    suspend fun getAllMaterials() = apiService.getAllMaterials()
    suspend fun getMaterialById(id: Int) = apiService.getMaterialById(id)
    suspend fun searchMaterials(name: String?, code: String?) = apiService.searchMaterials(name, code)
    suspend fun addMaterial(material: Material) = apiService.addMaterial(material)
    suspend fun editMaterial(id: Int, material: Material) = apiService.editMaterial(id, material)
    suspend fun deleteMaterial(id: Int) = apiService.deleteMaterial(id)
    
    // Warehouses
    suspend fun getAllWarehouses() = apiService.getAllWarehouses()
    suspend fun getWarehouseById(id: Int) = apiService.getWarehouseById(id)
    suspend fun searchWarehouses(name: String?, type: String?) = apiService.searchWarehouses(name, type)
    suspend fun addWarehouse(warehouse: Warehouse) = apiService.addWarehouse(warehouse)
    suspend fun editWarehouse(id: Int, warehouse: Warehouse) = apiService.editWarehouse(id, warehouse)
    suspend fun deleteWarehouse(id: Int) = apiService.deleteWarehouse(id)

    // Filling Warehouses
    suspend fun getAllFillingWarehouses() = apiService.getAllFillingWarehouses()
    suspend fun getFillingByMaterial(warehouseId: Int, materialId: Int) =
        apiService.getFillingByMaterial(warehouseId, materialId)
    suspend fun getFillingByProduct(warehouseId: Int, productId: Int) =
        apiService.getFillingByProduct(warehouseId, productId)
    suspend fun getFillingsByWarehouse(warehouseId: Int) = apiService.getFillingsByWarehouse(warehouseId)
    suspend fun getFillingsByMaterial(materialId: Int) = apiService.getFillingsByMaterial(materialId)
    suspend fun getFillingsByProduct(productId: Int) = apiService.getFillingsByProduct(productId)
    suspend fun addFillingWarehouse(filling: FillingWarehouse) = apiService.addFillingWarehouse(filling)
    suspend fun editFillingWarehouseByMaterial(warehouseId: Int, materialId: Int, filling: FillingWarehouse) =
        apiService.editFillingWarehouseByMaterial(warehouseId, materialId, filling)
    suspend fun editFillingWarehouseByProduct(warehouseId: Int, productId: Int, filling: FillingWarehouse) =
        apiService.editFillingWarehouseByProduct(warehouseId, productId, filling)
    suspend fun deleteFillingWarehouseByMaterial(warehouseId: Int, materialId: Int) =
        apiService.deleteFillingWarehouseByMaterial(warehouseId, materialId)
    suspend fun deleteFillingWarehouseByProduct(warehouseId: Int, productId: Int) =
        apiService.deleteFillingWarehouseByProduct(warehouseId, productId)
    suspend fun updateFillingQuantityByMaterial(filling: FillingWarehouse) =
        apiService.updateFillingQuantityByMaterial(filling)
    suspend fun updateFillingQuantityByProduct(filling: FillingWarehouse) =
        apiService.updateFillingQuantityByProduct(filling)
    
    // Work Reports
    suspend fun getAllWorkReports() = apiService.getAllWorkReports()
    suspend fun getWorkReportById(id: Int) = apiService.getWorkReportById(id)
    suspend fun getWorkReportsByUser(userId: Int) = apiService.getWorkReportsByUser(userId)
    suspend fun getActiveWorkReports(userId: Int) = apiService.getActiveWorkReports(userId)
    suspend fun addWorkReport(report: WorkReport) = apiService.addWorkReport(report)
    suspend fun startWork(request: StartWorkRequest) = apiService.startWork(request)
    suspend fun finishWork(id: Int, request: FinishWorkRequest?) = apiService.finishWork(id, request)
    
    // Part Requests
    suspend fun getAllPartRequests() = apiService.getAllPartRequests()
    suspend fun getPartRequestById(id: Int) = apiService.getPartRequestById(id)
    suspend fun addPartRequest(request: PartRequest) = apiService.addPartRequest(request)
    suspend fun approvePartRequest(id: Int) = apiService.approvePartRequest(id)
    suspend fun rejectPartRequest(id: Int, reason: String?) = apiService.rejectPartRequest(id, reason)
    
    // Alarm Events
    suspend fun getAllAlarmEvents() = apiService.getAllAlarmEvents()
    suspend fun getAlarmEventsByUser(userId: Int) = apiService.getAlarmEventsByUser(userId)
    suspend fun addAlarmEvent(event: AlarmEvent) = apiService.addAlarmEvent(event)
}

