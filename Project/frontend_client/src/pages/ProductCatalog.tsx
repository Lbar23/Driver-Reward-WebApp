import { FC, useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Checkbox,
  FormControlLabel,
  FormGroup,
  TextField,
  Typography,
  Card,
  CardContent,
  CardMedia,
  Button,
  Grid,
  Box,
  Container,
  Chip,
  CircularProgress,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import axios from "axios";
import { useAuth } from "../service/authContext";

interface Product {
  sponsorID: number;
  productID: number;
  productName: string;
  category: string;
  description: string;
  currencyPrice: number;
  priceInPoints: number;
  externalID: string;
  imageUrl: string;
  availability: boolean;
}

interface Category {
  id: number;
  name: string;
}

interface CatalogPageProps {
  sponsorId: number;
}

const CATEGORIES: Category[] = [
  { id: 6030, name: "Accessories" },
  { id: 293, name: "Electronics" },
  { id: 11450, name: "Clothing" },
  { id: 11700, name: "Home & Garden" },
];

const CatalogPage: FC = () => {
  const { user } = useAuth();
  const [showOverlay, setShowOverlay] = useState<boolean>(false);
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch products when the page loads or when a catalog is created
  useEffect(() => {
    if (user?.sponsorDetails?.sponsorID) {
      console.log("Triggering fetchProducts in useEffect");
      fetchProducts();
    } else {
      console.warn("No sponsorID found, fetchProducts not triggered");
    }
  }, [user?.sponsorDetails?.sponsorID]);
  

  const fetchProducts = async () => {
    if (!user?.sponsorDetails?.sponsorID) {
      console.warn("No sponsor ID found for user:", user);
      return;
    }
  
    try {
      setIsLoading(true);
      console.log("Fetching products for sponsor ID:", user.sponsorDetails.sponsorID);
  
      const response = await axios.get<Product[]>(`/api/catalog/${user.sponsorDetails.sponsorID}`);
      console.log("API response data:", response.data);
  
      // Check if response data matches expected structure
      if (Array.isArray(response.data) && response.data.length > 0) {
        setProducts(response.data);
        setError(null);
      } else {
        console.warn("API returned unexpected data format:", response.data);
        setError("Unexpected response format. Please contact support.");
      }
    } catch (err) {
      console.error("Error fetching products:", err);
      setError("Failed to fetch products. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };
  

  const handleCatalogCreated = () => {
    fetchProducts();
  };

  const formatPrice = (price: number): string =>
    new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(price);

  const formatPoints = (points: number): string =>
    new Intl.NumberFormat("en-US").format(points);

  // Overlay for creating a catalog
  const CatalogOverlay: FC = () => {
    const [selectedCategories, setSelectedCategories] = useState<number[]>([]);
    const [numberOfProducts, setNumberOfProducts] = useState<number>(10);
    const [pointValue, setPointValue] = useState<number>(100);
    const [overlayError, setOverlayError] = useState<string | null>(null);

    const handleCategoryChange = (categoryId: number) => {
      setSelectedCategories((prev) =>
        prev.includes(categoryId)
          ? prev.filter((c) => c !== categoryId)
          : [...prev, categoryId]
      );
    };

    const handleCreateCatalog = async () => {
      if (selectedCategories.length === 0) {
        setOverlayError("Please select at least one category.");
        return;
      }
      setOverlayError(null);
      setIsLoading(true);

      try {
        for (const categoryId of selectedCategories) {
          const queryParams = new URLSearchParams({
            categoryId: categoryId.toString(),
            numberOfProducts: numberOfProducts.toString(),
            pointValue: pointValue.toString(),
          });

          await axios.post(
            `/api/catalog/create?${queryParams}`,
            {},
            { withCredentials: true }
          );
        }

        handleCatalogCreated();
        setShowOverlay(false);
      } catch (err) {
        console.error("Error creating catalog:", err);
        setOverlayError("Failed to create catalog. Please try again.");
      } finally {
        setIsLoading(false);
      }
    };

    return (
      <Dialog open={true} onClose={() => setShowOverlay(false)}>
        <DialogTitle>Create a New Catalog</DialogTitle>
        <DialogContent>
          <Typography variant="body1" gutterBottom>
            Select categories and configure catalog details.
          </Typography>
          <FormGroup>
            {CATEGORIES.map((category) => (
              <FormControlLabel
                key={category.id}
                control={
                  <Checkbox
                    checked={selectedCategories.includes(category.id)}
                    onChange={() => handleCategoryChange(category.id)}
                  />
                }
                label={category.name}
              />
            ))}
          </FormGroup>
          <TextField
            label="Number of Products"
            type="number"
            fullWidth
            margin="normal"
            value={numberOfProducts}
            onChange={(e) => setNumberOfProducts(Number(e.target.value))}
          />
          <TextField
            label="Point Value"
            type="number"
            fullWidth
            margin="normal"
            value={pointValue}
            onChange={(e) => setPointValue(Number(e.target.value))}
          />
          {overlayError && (
            <Typography variant="body2" color="error">
              {overlayError}
            </Typography>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowOverlay(false)} color="inherit">
            Cancel
          </Button>
          <Button
            onClick={handleCreateCatalog}
            disabled={isLoading}
            variant="contained"
            color="primary"
          >
            {isLoading ? "Creating..." : "Create Catalog"}
          </Button>
        </DialogActions>
      </Dialog>
    );
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 4 }}>
        <Typography variant="h4" component="h1">
          Product Catalog
        </Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => setShowOverlay(true)}
        >
          Create New Catalog
        </Button>
      </Box>

      {error && (
        <Typography color="error" align="center">
          {error}
        </Typography>
      )}

      {isLoading ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          {products.length === 0 ? (
            <Grid item xs={12}>
              <Typography align="center">No products in catalog.</Typography>
            </Grid>
          ) : (
            products.map((product) => (
              <Grid item xs={12} sm={6} md={4} lg={3} key={product.productID}>
                <Card>
                  <CardMedia
                    component="img"
                    height="200"
                    image={product.imageUrl}
                    alt={product.productName}
                  />
                  <CardContent>
                    <Typography variant="h6">{product.productName}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {product.description}
                    </Typography>
                    <Box mt={2} display="flex" justifyContent="space-between">
                      <Typography>{formatPrice(product.currencyPrice)}</Typography>
                      {product.priceInPoints > 0 && (
                        <Chip label={`${formatPoints(product.priceInPoints)} pts`} color="primary" />
                      )}
                    </Box>
                    <Button
                      variant="contained"
                      startIcon={<ShoppingCartIcon />}
                      fullWidth
                      disabled={!product.availability}
                      sx={{ mt: 2 }}
                    >
                      {product.availability ? "Add to Cart" : "Out of Stock"}
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            ))
          )}
        </Grid>
      )}

      {showOverlay && <CatalogOverlay />}
    </Container>
  );
};

export default CatalogPage;
