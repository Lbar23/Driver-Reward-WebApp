import React, { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Checkbox,
  FormControlLabel,
  FormGroup,
  TextField,
  Button,
  Typography,
} from "@mui/material";
import axios from "axios";

const CATEGORIES = [
  { id: 6030, name: "Accessories" },
  { id: 293, name: "Electronics" },
  { id: 11450, name: "Clothing" },
  { id: 11700, name: "Home & Garden" },
];

interface CatalogOverlayProps {
  sponsorId: number;
  onClose: () => void;
  onCatalogCreated: () => void;
}

const CatalogOverlay: React.FC<CatalogOverlayProps> = ({onClose, onCatalogCreated }) => {
  const [selectedCategories, setSelectedCategories] = useState<number[]>([]);
  const [numberOfProducts, setNumberOfProducts] = useState(10);
  const [pointValue, setPointValue] = useState(100);
  const [error, setError] = useState<string | null>(null);
  const handleCategoryChange = (category: number) => {
    setSelectedCategories((prev) =>
      prev.includes(category)
        ? prev.filter((c) => c !== category)
        : [...prev, category]
    );
  };

  const handleCreateCatalog = async () => {
    if (selectedCategories.length === 0) {
      setError("Please select at least one category.");
      return;
    }
    setError(null);

    try {
      const queryParams = new URLSearchParams({
        categories: selectedCategories.join(","),
        numberOfProducts: numberOfProducts.toString(),
      });

      const response = await axios.post(
        `/api/catalog/create?${queryParams}`
      );

      if (response.status === 200) {
        onCatalogCreated(); // Notify parent of successful catalog creation
        onClose(); // Close the dialog
      }
    } catch (err) {
      console.error("Error creating catalog:", err);
      setError("Failed to create catalog. Please try again.");
    }
  };

  return (
    <Dialog open onClose={onClose}>
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
        {error && (
          <Typography variant="body2" color="error">
            {error}
          </Typography>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="secondary">
          Cancel
        </Button>
        <Button onClick={handleCreateCatalog} color="primary" variant="contained">
          Create Catalog
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default CatalogOverlay;
