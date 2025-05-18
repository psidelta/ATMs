# ATM Network Optimization Algorithm

## Overview
This document details the mathematical optimization algorithm used for ATM network planning. The algorithm utilizes integer linear programming to determine optimal ATM placement while considering multiple operational constraints and business objectives.

## Input Parameters

### Configuration Parameters
- **α (alpha)**: Weight coefficient for distance-based constraints
- **β (beta)**: Weight coefficient for transaction volume
- **δ (delta)**: Weight coefficient for operational costs
- **γ (gamma)**: Transaction threshold parameter
- **ε₁-ε₄ (epsilon)**: Fine-tuning parameters for optimization model
- **Age Function Parameters**:
  - Mean: Center point for age-based calculations
  - Slope: Rate of change for age-based impact
- **Minimum Transaction Count**: Threshold for mandatory ATM retention

### Input Data
1. **ATM Data**:
   - Location coordinates
   - Transaction volumes
   - Operating costs
   - Age of machines
   - ATM types
   - Certifiability status

2. **Geographic Data**:
   - Distance matrix between ATMs
   - Geocoding information
   - Postal codes
   - City designators

## Algorithm Steps

### 1. Data Preprocessing
```
1.1 Load configuration from appsettings.json
1.2 Read CSV input data
1.3 Generate distance matrix using Google API
1.4 Create data encoding mappings for optimization
1.5 Convert input data into optimization-ready arrays
```

### 2. Mathematical Formulation

#### Decision Variables
- x[j] ∈ {0,1} for each ATM j
  - x[j] = 1 if ATM is kept
  - x[j] = 0 if ATM is removed

#### Parameters
- α (alpha): Maximum distance for primary coverage
- β (beta): Maximum distance for transaction redistribution
- δ (delta): Distance power coefficient for transaction distribution
- γ (gamma): Transaction volume threshold
- ε₁-ε₄ (epsilon): Weight coefficients for objective function components
- Age function parameters:
  - μ (mean): Center point for sigmoid function
  - k (slope): Steepness of sigmoid curve

#### Indicator Functions

1. **Distance-Based Coverage Indicator**:
```
IndicatorAlpha(i,j) = {
    1  if distance(i,j) ≤ α
    0  otherwise
}
```

2. **Transaction Distribution Indicator**:
```
IndicatorBeta(i,j) = {
    1  if distance(i,j) ≤ β
    0  otherwise
}
```

#### Constraints

1. **Blocked ATM Constraints**:
```
x[j] = 1  ∀j ∈ BlockedATMs
```
**Purpose**: Ensures certain ATMs must remain in the network
- Maintains service in strategic locations
- Preserves ATMs with contractual obligations
- Keeps ATMs in locations where removal is not an option (e.g., bank branches)
**Impact**: These ATMs are fixed points in the solution and reduce the solution space

2. **High Transaction Volume Constraints**:
```
x[j] = 1  ∀j where TrxCnt[j] ≥ minStayTrxCnt
```
**Purpose**: Preserves high-performing ATMs
- Protects ATMs with proven high usage
- Prevents removal of profitable locations
- Maintains service at popular locations
**Impact**: Ensures the optimization doesn't remove ATMs that are clearly valuable to the network

3. **Coverage Constraints**:
```
Σ(x[j] * IndicatorAlpha(i,j) * I(TrxCnt[j] ≤ γ)) + x[i] ≥ 0.1  ∀i
where:
- I(condition) is an indicator function: 1 if true, 0 if false
- IndicatorAlpha(i,j) = 1 if distance(i,j) ≤ α
```
**Purpose**: Ensures adequate geographic coverage
- Prevents creation of service deserts
- Guarantees minimum service level in all areas
- Considers only ATMs below transaction threshold γ for coverage
**Impact**: Forces the solution to maintain a minimum number of ATMs within distance α of any point

#### Objective Function Components

1. **Profit Coefficient**:
```
ProfitCoeff[i] = (Cost[i]/TrxCnt[i] - minProfitCoeff) / (maxProfitCoeff - minProfitCoeff)
```
**Purpose**: Measures operational efficiency
- Higher values indicate higher cost per transaction
- Normalized to [0,1] range for fair comparison
- Positive weight (ε₁) means we prefer to remove high-cost ATMs
**Impact**: Drives removal of ATMs with poor cost-to-transaction ratios

2. **Age Coefficient**:
```
AgeCoeff[i] = 1 - 1/(1 + e^(k * (-(age[i] - μ))))
```
**Purpose**: Considers ATM age in decision making
- Uses sigmoid function for smooth transition
- μ (mean) centers the curve at desired age point
- k (slope) controls how sharply age impacts decision
- Negative weight (-ε₂) means we prefer to remove older ATMs
**Impact**: Encourages replacement of aging infrastructure

3. **Type Coefficient**:
```
TypeCoeff[i] = TypeArray[i]
```
**Purpose**: Accounts for ATM type/capabilities
- Different weights for different ATM types
- Considers advanced features (e.g., deposit capability)
- Positive weight (ε₃) means we prefer to keep advanced ATMs
**Impact**: Influences retention of ATMs with special features

4. **Certifiability Coefficient**:
```
CertCoeff[i] = CertArray[i]
```
**Purpose**: Considers regulatory compliance
- Higher values indicate easier certification
- Negative weight (-ε₄) encourages keeping easily certified ATMs
**Impact**: Influences retention of ATMs that are easier to maintain compliance

#### Complete Objective Function
```
Minimize: Σ(i) { x[i] * (
    ε₁ * ProfitCoeff[i] 
    - ε₂ * AgeCoeff[i] 
    + ε₃ * TypeCoeff[i] 
    - ε₄ * CertCoeff[i]
)}
```

#### Transaction Redistribution Model

The model predicts how transactions will redistribute after ATM removals:
```
NewTrxCnt[i] = TrxCnt[i] + Σ(j≠i) { 
    TrxCnt[j] * IndicatorBeta(i,j) * (1-x[j]) * d(j,i)^δ / D[j]
}
```
**Components**:
- Original transactions (TrxCnt[i])
- Redistributed transactions from removed ATMs
- Distance-based distribution (d(j,i)^δ)
- Coverage indicator (IndicatorBeta)
- Normalization factor (D[j])

**Purpose**:
- Models customer behavior after ATM removal
- Accounts for distance preference in ATM choice
- Ensures realistic transaction redistribution
- Considers maximum redistribution distance (β)

**Impact**:
- Helps predict network performance after optimization
- Influences decisions through transaction volume constraints
- Provides realistic assessment of network changes

### 3. Solution Process

#### Initial Solution
```
1. Initialize SCIP solver
2. Define decision variables
3. Add all constraints
4. Set objective function
5. Solve initial optimization problem
```

#### Iterative Refinement
```
While (solution needs improvement):
    1. Get current solution vector
    2. Calculate new transaction counts
    3. Update constraints if needed
    4. Resolve optimization problem
    5. Check convergence criteria
```

## Output

The algorithm produces:
1. Binary decision vector indicating which ATMs to keep/remove
2. Updated transaction distribution
3. Coverage analysis
4. Cost implications

## Implementation Notes

### Technology Stack
- Google OR-Tools with SCIP solver
- C# implementation
- Google Maps API for distance calculations

### Performance Considerations
- Matrix operations are optimized for large-scale networks
- Iterative process ensures transaction volume constraints are met
- Efficient data structures minimize memory usage

## Limitations and Constraints
1. Solution optimality depends on input data quality
2. Geographic constraints must be carefully calibrated
3. Transaction redistribution model assumes rational customer behavior
4. Computational complexity increases with network size

## Future Enhancements
1. Dynamic parameter adjustment
2. Multi-period optimization
3. Stochastic demand modeling
4. Real-time constraint updates
