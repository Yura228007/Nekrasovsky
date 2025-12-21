package com.nekrasovsky.android.ui.screens.warehouses

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.nekrasovsky.android.data.model.Warehouse
import com.nekrasovsky.android.data.repository.AppRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@Composable
fun WarehousesScreen(
    onNavigateBack: () -> Unit,
    viewModel: WarehousesViewModel = viewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Склады", fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton(onClick = onNavigateBack) {
                        Icon(Icons.Default.ArrowBack, "Назад")
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.loadWarehouses() }) {
                        Icon(Icons.Default.Refresh, "Обновить")
                    }
                }
            )
        },
        floatingActionButton = {
            FloatingActionButton(onClick = { /* TODO: Add warehouse */ }) {
                Icon(Icons.Default.Add, "Добавить")
            }
        }
    ) { paddingValues ->
        when {
            uiState.isLoading -> {
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues),
                    contentAlignment = Alignment.Center
                ) {
                    CircularProgressIndicator()
                }
            }
            uiState.error != null -> {
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues),
                    contentAlignment = Alignment.Center
                ) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text(
                            text = "Ошибка: ${uiState.error}",
                            color = MaterialTheme.colorScheme.error
                        )
                        Spacer(modifier = Modifier.height(16.dp))
                        Button(onClick = { viewModel.loadWarehouses() }) {
                            Text("Повторить")
                        }
                    }
                }
            }
            else -> {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(paddingValues),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(uiState.warehouses) { warehouse ->
                        WarehouseCard(warehouse = warehouse)
                    }
                }
            }
        }
    }
}

@Composable
fun WarehouseCard(warehouse: Warehouse) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp)
        ) {
            Text(
                text = warehouse.name,
                style = MaterialTheme.typography.titleLarge,
                fontWeight = FontWeight.Bold
            )
            Text(
                text = "Тип: ${warehouse.type}",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.primary,
                modifier = Modifier.padding(top = 4.dp)
            )
            if (warehouse.location != null) {
                Text(
                    text = "Местоположение: ${warehouse.location}",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(top = 4.dp)
                )
            }
            if (warehouse.description != null) {
                Text(
                    text = warehouse.description,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.padding(top = 4.dp)
                )
            }
        }
    }
}

@HiltViewModel
class WarehousesViewModel @Inject constructor(
    private val repository: AppRepository
) : androidx.lifecycle.ViewModel() {
    
    private val _uiState = MutableStateFlow(WarehousesUiState())
    val uiState: StateFlow<WarehousesUiState> = _uiState.asStateFlow()
    
    init {
        loadWarehouses()
    }
    
    fun loadWarehouses() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val response = repository.getAllWarehouses()
                if (response.isSuccessful) {
                    _uiState.value = _uiState.value.copy(
                        warehouses = response.body() ?: emptyList(),
                        isLoading = false
                    )
                } else {
                    _uiState.value = _uiState.value.copy(
                        error = "Ошибка загрузки: ${response.code()}",
                        isLoading = false
                    )
                }
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    error = e.message ?: "Неизвестная ошибка",
                    isLoading = false
                )
            }
        }
    }
}

data class WarehousesUiState(
    val warehouses: List<Warehouse> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

