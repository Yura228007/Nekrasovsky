package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

enum class PartRequestStatus {
    PENDING,
    APPROVED,
    REJECTED
}

data class PartRequest(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("fromWarehouseId")
    val fromWarehouseId: Int,
    
    @SerializedName("toWarehouseId")
    val toWarehouseId: Int,
    
    @SerializedName("materialId")
    val materialId: Int,
    
    @SerializedName("quantity")
    val quantity: Double,
    
    @SerializedName("sentByUserId")
    val sentByUserId: Int,
    
    @SerializedName("receivedByUserId")
    val receivedByUserId: Int? = null,
    
    @SerializedName("status")
    val status: String = "PENDING",
    
    @SerializedName("requestDate")
    val requestDate: String = "",
    
    @SerializedName("rejectionReason")
    val rejectionReason: String? = null
)

