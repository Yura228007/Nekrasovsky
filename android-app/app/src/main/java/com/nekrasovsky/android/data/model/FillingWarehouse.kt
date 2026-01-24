package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class FillingWarehouse(
    @SerializedName("id")
    val id: Int = 0,

    @SerializedName("warehouseId")
    val warehouseId: Int,

    @SerializedName("materialId")
    val materialId: Int? = null,

    @SerializedName("productId")
    val productId: Int? = null,

    @SerializedName("quantity")
    val quantity: Int = 0,

    @SerializedName("measuringType")
    val measuringType: String? = null
)
