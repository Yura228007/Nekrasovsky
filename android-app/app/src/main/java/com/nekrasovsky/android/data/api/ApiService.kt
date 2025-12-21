package com.nekrasovsky.android.data.api

import com.nekrasovsky.android.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {
    
    // Users
    @GET("api/users")
    suspend fun getAllUsers(): Response<List<User>>
    
    @GET("api/users/{id}")
    suspend fun getUserById(@Path("id") id: Int): Response<User>
    
    @GET("api/users/search")
    suspend fun searchUsers(
        @Query("name") name: String? = null,
        @Query("surname") surname: String? = null
    ): Response<List<User>>
    
    @POST("api/users/add")
    suspend fun addUser(@Body user: User): Response<ApiResponse<User>>
    
    @POST("api/users/edit/{id}")
    suspend fun editUser(@Path("id") id: Int, @Body user: User): Response<ApiResponse<User>>
    
    @POST("api/users/delete/{id}")
    suspend fun deleteUser(@Path("id") id: Int): Response<ApiResponse<Unit>>
    
    // Products
    @GET("api/products")
    suspend fun getAllProducts(): Response<List<Product>>
    
    @GET("api/products/{id}")
    suspend fun getProductById(@Path("id") id: Int): Response<Product>
    
    @GET("api/products/search")
    suspend fun searchProducts(
        @Query("name") name: String? = null,
        @Query("code") code: String? = null
    ): Response<List<Product>>
    
    @POST("api/products/add")
    suspend fun addProduct(@Body product: Product): Response<ApiResponse<Product>>
    
    @POST("api/products/edit/{id}")
    suspend fun editProduct(@Path("id") id: Int, @Body product: Product): Response<ApiResponse<Product>>
    
    @POST("api/products/delete/{id}")
    suspend fun deleteProduct(@Path("id") id: Int): Response<ApiResponse<Unit>>
    
    // Materials
    @GET("api/materials")
    suspend fun getAllMaterials(): Response<List<Material>>
    
    @GET("api/materials/{id}")
    suspend fun getMaterialById(@Path("id") id: Int): Response<Material>
    
    @GET("api/materials/search")
    suspend fun searchMaterials(
        @Query("name") name: String? = null,
        @Query("code") code: String? = null
    ): Response<List<Material>>
    
    @POST("api/materials/add")
    suspend fun addMaterial(@Body material: Material): Response<ApiResponse<Material>>
    
    @POST("api/materials/edit/{id}")
    suspend fun editMaterial(@Path("id") id: Int, @Body material: Material): Response<ApiResponse<Material>>
    
    @POST("api/materials/delete/{id}")
    suspend fun deleteMaterial(@Path("id") id: Int): Response<ApiResponse<Unit>>
    
    // Warehouses
    @GET("api/warehouses")
    suspend fun getAllWarehouses(): Response<List<Warehouse>>
    
    @GET("api/warehouses/{id}")
    suspend fun getWarehouseById(@Path("id") id: Int): Response<Warehouse>
    
    @GET("api/warehouses/search")
    suspend fun searchWarehouses(
        @Query("name") name: String? = null,
        @Query("type") type: String? = null
    ): Response<List<Warehouse>>
    
    @POST("api/warehouses/add")
    suspend fun addWarehouse(@Body warehouse: Warehouse): Response<ApiResponse<Warehouse>>
    
    @POST("api/warehouses/edit/{id}")
    suspend fun editWarehouse(@Path("id") id: Int, @Body warehouse: Warehouse): Response<ApiResponse<Warehouse>>
    
    @POST("api/warehouses/delete/{id}")
    suspend fun deleteWarehouse(@Path("id") id: Int): Response<ApiResponse<Unit>>
    
    // Work Reports
    @GET("api/work-reports")
    suspend fun getAllWorkReports(): Response<List<WorkReport>>
    
    @GET("api/work-reports/{id}")
    suspend fun getWorkReportById(@Path("id") id: Int): Response<WorkReport>
    
    @GET("api/work-reports/user/{userId}")
    suspend fun getWorkReportsByUser(@Path("userId") userId: Int): Response<List<WorkReport>>
    
    @GET("api/work-reports/user/{userId}/active")
    suspend fun getActiveWorkReports(@Path("userId") userId: Int): Response<List<WorkReport>>
    
    @POST("api/work-reports/add")
    suspend fun addWorkReport(@Body report: WorkReport): Response<ApiResponse<WorkReport>>
    
    @POST("api/work-reports/start")
    suspend fun startWork(@Body request: StartWorkRequest): Response<ApiResponse<WorkReport>>
    
    @POST("api/work-reports/{id}/finish")
    suspend fun finishWork(@Path("id") id: Int, @Body request: FinishWorkRequest?): Response<ApiResponse<WorkReport>>
    
    // Part Requests
    @GET("api/part-requests")
    suspend fun getAllPartRequests(): Response<List<PartRequest>>
    
    @GET("api/part-requests/{id}")
    suspend fun getPartRequestById(@Path("id") id: Int): Response<PartRequest>
    
    @POST("api/part-requests/add")
    suspend fun addPartRequest(@Body request: PartRequest): Response<ApiResponse<PartRequest>>
    
    @POST("api/part-requests/{id}/approve")
    suspend fun approvePartRequest(@Path("id") id: Int): Response<ApiResponse<PartRequest>>
    
    @POST("api/part-requests/{id}/reject")
    suspend fun rejectPartRequest(@Path("id") id: Int, @Body reason: String?): Response<ApiResponse<PartRequest>>
    
    // Alarm Events
    @GET("api/alarm-events")
    suspend fun getAllAlarmEvents(): Response<List<AlarmEvent>>
    
    @GET("api/alarm-events/user/{userId}")
    suspend fun getAlarmEventsByUser(@Path("userId") userId: Int): Response<List<AlarmEvent>>
    
    @POST("api/alarm-events/add")
    suspend fun addAlarmEvent(@Body event: AlarmEvent): Response<ApiResponse<AlarmEvent>>
}

data class ApiResponse<T>(
    val message: String,
    val user: T? = null,
    val product: T? = null,
    val material: T? = null,
    val warehouse: T? = null,
    val report: T? = null,
    val request: T? = null,
    val alarmEvent: T? = null
)

